using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace EGOIST.Domain.Structures;

/// <summary>
/// Represents a dictionary that supports observation of changes through the <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/> interfaces.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, INotifyCollectionChanged,
    INotifyPropertyChanged, ISerializable
{
    private class Node : INotifyPropertyChanged
    {
        private readonly TKey _key;
        private TValue? _value;
        private Node? _next;
        private Node? _previous;

        public TKey Key
        {
            get => _key;
            init
            {
                if (Equals(_key, value)) return;
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        public TValue? Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public Node? Next
        {
            get => _next;
            set
            {
                if (Equals(_next, value)) return;
                _next = value;
                OnPropertyChanged(nameof(Next));
            }
        }
        public Node? Previous
        {
            get => _previous;
            set
            {
                if (Equals(_previous, value)) return;
                _previous = value;
                OnPropertyChanged(nameof(Previous));
            }
        }

        public Node(TKey key, TValue value)
        {
            Key = key;
            _key = key;
            Value = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private Node? _head;
    private Node? _tail;
    private int _count;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly IEqualityComparer<TKey> _comparer;

    private ICollection<TKey>? _keys;
    private ICollection<TValue>? _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty, 
    /// has the default initial capacity, and uses the default equality comparer for the key type.
    /// </summary>
    public ObservableDictionary() : this(EqualityComparer<TKey>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty, 
    /// has the default initial capacity, and uses the specified <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, 
    /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the key.</param>
    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _count = 0;
        _comparer = comparer;
    }

    /// <summary>
    /// Gets or sets the element with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <returns>The element with the specified key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
    public TValue this[TKey key]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                var current = Head;
                while (current != null)
                {
                    if (_comparer.Equals(current.Key, key) && current.Value != null) 
                        return current.Value;

                    current = current.Next;
                }

                throw new KeyNotFoundException();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                var current = Head;
                while (current != null)
                {
                    if (_comparer.Equals(current.Key, key))
                    {
                        var oldValue = current.Value;
                        current.Value = value;
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<TKey, TValue>(key, value),
                            new KeyValuePair<TKey, TValue?>(key, oldValue)));
                        OnPropertyChanged("Item[]");
                        return;
                    }

                    current = current.Next;
                }

                Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    public ICollection<TKey> Keys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _keys ??= new Collection<TKey>(this.Select(item => item.Key).ToList());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    public ICollection<TValue> Values
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _values ??= new Collection<TValue>(this.Select(item => item.Value).ToList());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void CopyTo(Array array, int index)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (var item in this)
            {
                array.SetValue(item, index++);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    
    
    /// <summary>
    /// Gets or sets the head node of the dictionary.
    /// </summary>
    /// <value>The head node.</value>
    /// <remarks>This property is thread-safe.</remarks>
    private Node? Head
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _head;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                _head = value;
                OnPropertyChanged(nameof(Head));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the tail node of the dictionary.
    /// </summary>
    /// <value>The tail node.</value>
    /// <remarks>This property is thread-safe.</remarks>
    private Node? Tail
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _tail;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                _tail = value;
                OnPropertyChanged(nameof(Tail));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// Gets the number of key/value pairs contained in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool IsSynchronized { get; } = true;
    public object SyncRoot { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="ObservableDictionary{TKey, TValue}"/>.</exception>
    public void Add(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists.");
            }

            Node newNode = new Node(key, value);

            if (Tail != null)
            {
                Tail.Next = newNode;
                newNode.Previous = Tail;
                Tail = newNode;
            }
            else
            {
                Head = newNode;
                Tail = newNode;
            }

            _count++;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                new KeyValuePair<TKey, TValue>(key, value)));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            _keys = null; // Invalidate cached keys
            _values = null; // Invalidate cached values
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Adds an item to the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="item">The key/value pair to add to the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Adds a range of elements to the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="items">The key/value pairs to add to the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        _lock.EnterWriteLock();
        try
        {
            List<KeyValuePair<TKey, TValue>> addedItems = new List<KeyValuePair<TKey, TValue>>();
            foreach (var item in items)
            {
                if (!ContainsKey(item.Key))
                {
                    Node newNode = new Node(item.Key, item.Value);
                    if (Tail != null)
                    {
                        Tail.Next = newNode;
                        newNode.Previous = Tail;
                        Tail = newNode;
                    }
                    else
                    {
                        Head = newNode;
                        Tail = newNode;
                    }

                    _count++;
                    addedItems.Add(item);
                }
            }

            if (addedItems.Count > 0)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                _keys = null; // Invalidate cached keys
                _values = null; // Invalidate cached values
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes all items from the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            Head = null;
            Tail = null;
            _count = 0;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            _keys = null; // Invalidate cached keys
            _values = null; // Invalidate cached values
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    /// <returns>true if <paramref name="item"/> is found in the <see cref="ObservableDictionary{TKey, TValue}"/>; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            while (current != null)
            {
                if (current.Value != null && _comparer.Equals(current.Key, item.Key) && current.Value.Equals(item.Value))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    /// <returns>true if the <see cref="ObservableDictionary{TKey, TValue}"/> contains an element with the key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public bool ContainsKey(TKey key)
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Copies the elements of the <see cref="ObservableDictionary{TKey, TValue}"/> to an <see cref="Array"/>, 
    /// starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// the <see cref="ObservableDictionary{TKey, TValue}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">The number of elements in the source <see cref="ObservableDictionary{TKey, TValue}"/> 
    /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            var index = arrayIndex;
            while (current != null)
            {
                if (current.Value != null) array[index++] = new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Next;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            while (current != null)
            {
                if (current.Value != null) yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Next;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes the element with the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully removed; otherwise, false. 
    /// This method also returns false if <paramref name="key"/> was not found in the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public bool Remove(TKey key)
    {
        _lock.EnterWriteLock();
        try
        {
            if (Head == null)
            {
                return false;
            }

            if (_comparer.Equals(Head.Key, key))
            {
                var oldValue = Head.Value;
                Head = Head.Next;
                if (Head != null)
                {
                    Head.Previous = null;
                }
                else
                {
                    Tail = null; // If removing the only element
                }

                _count--;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    new KeyValuePair<TKey, TValue?>(key, oldValue)));
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                _keys = null; // Invalidate cached keys
                _values = null; // Invalidate cached values
                return true;
            }

            var current = Head.Next;
            while (current != null && !_comparer.Equals(current.Key, key))
            {
                current = current.Next;
            }

            if (current != null)
            {
                var oldValue = current.Value;
                if (current.Previous != null)
                {
                    current.Previous.Next = current.Next;
                    if (current.Next != null)
                    {
                        current.Next.Previous = current.Previous;
                    }
                    else
                    {
                        Tail = current.Previous; // If removing the last element
                    }
                }

                _count--;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    new KeyValuePair<TKey, TValue?>(key, oldValue)));
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                _keys = null; // Invalidate cached keys
                _values = null; // Invalidate cached values
                return true;
            }

            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    /// <returns>true if <paramref name="item"/> was successfully removed from the <see cref="ObservableDictionary{TKey, TValue}"/>; 
    /// otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the <see cref="ObservableDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public bool TryGetValue(TKey key, out TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key) && current.Value != null)
                {
                    value = current.Value;
                    return true;
                }

                current = current.Next;
            }

            value = default!;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Begins a transaction that prevents the <see cref="ObservableDictionary{TKey, TValue}"/> from raising change notifications until <see cref="CommitTransaction"/> is called.
    /// </summary>
    public void BeginTransaction() => _lock.EnterWriteLock();

    /// <summary>
    /// Commits the transaction started by <see cref="BeginTransaction"/> and raises a single <see cref="NotifyCollectionChangedAction.Reset"/> event to notify listeners of all changes made during the transaction.
    /// </summary>
    public void CommitTransaction()
    {
        _lock.ExitWriteLock();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged("Count");
        OnPropertyChanged("Item[]");
        _keys = null; // Invalidate cached keys
        _values = null; // Invalidate cached values
    }

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with the provided arguments.
    /// </summary>
    /// <param name="e">Arguments of the event being raised.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) =>
        CollectionChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event with the provided property name.
    /// </summary>
    /// <param name="propertyName">Name of the property that has changed.</param>
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Serialization Support
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the dictionary being deserialized.</param>
    protected ObservableDictionary(SerializationInfo info)
    {
        _comparer = (IEqualityComparer<TKey>)info.GetValue("Comparer", typeof(IEqualityComparer<TKey>)) ?? EqualityComparer<TKey>.Default;
        _count = info.GetInt32("Count");

        var items =
            (KeyValuePair<TKey, TValue>[])info.GetValue("Items", typeof(KeyValuePair<TKey, TValue>[]));

        if (items == null) return;
        foreach (var item in items)
        {
            Add(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="ISerializable"/> interface and returns the data needed to serialize the <see cref="ObservableDictionary{TKey, TValue}"/> instance.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> object that contains the information required to serialize the <see cref="ObservableDictionary{TKey, TValue}"/> instance.</param>
    /// <param name="context">A <see cref="StreamingContext"/> structure that contains the source and destination of the serialized stream associated with the <see cref="ObservableDictionary{TKey, TValue}"/> instance.</param>
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Comparer", _comparer, typeof(IEqualityComparer<TKey>));
        info.AddValue("Count", _count);

        KeyValuePair<TKey, TValue>[] items = this.ToArray();
        info.AddValue("Items", items, typeof(KeyValuePair<TKey, TValue>[]));
    }

    // IDictionary implementation
    bool IDictionary.IsFixedSize => false;
    bool IDictionary.IsReadOnly => IsReadOnly;
    ICollection IDictionary.Keys => Keys.Cast<object>().ToList();
    ICollection IDictionary.Values => Values.Cast<object>().ToList();

    object? IDictionary.this[object key]
    {
        get
        {
            if (key is TKey tkey && ContainsKey(tkey))
            {
                return this[tkey];
            }

            return null;
        }
        set
        {
            if (key is TKey tkey && value is TValue tvalue)
            {
                this[tkey] = tvalue;
            }
            else
            {
                throw new ArgumentException("Invalid key or value type.");
            }
        }
    }

    void IDictionary.Add(object key, object? value)
    {
        if (key is TKey tkey && value is TValue tvalue)
        {
            Add(tkey, tvalue);
        }
        else
        {
            throw new ArgumentException("Invalid key or value type.");
        }
    }

    bool IDictionary.Contains(object key)
    {
        if (key is TKey tkey)
        {
            return ContainsKey(tkey);
        }

        return false;
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return new DictionaryEnumerator(GetEnumerator());
    }

    void IDictionary.Remove(object key)
    {
        if (key is TKey tkey)
        {
            Remove(tkey);
        }
    }

    /// <summary>
    /// Returns the index of the first occurrence of the specified key in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    /// <returns>The index of the first occurrence of the specified key in the <see cref="ObservableDictionary{TKey, TValue}"/>, 
    /// or -1 if the key is not found.</returns>
    public int IndexOfKey(TKey key)
    {
        _lock.EnterReadLock();
        try
        {
            var index = 0;
            var current = Head;
            while (current != null)
            {
                if (_comparer.Equals(current.Key, key))
                {
                    return index;
                }

                current = current.Next;
                index++;
            }

            return -1;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Returns the index of the first occurrence of the specified value in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="value">The value to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.</param>
    /// <returns>The index of the first occurrence of the specified value in the <see cref="ObservableDictionary{TKey, TValue}"/>, 
    /// or -1 if the value is not found.</returns>
    public int IndexOfValue(TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            var index = 0;
            var current = Head;
            while (current != null)
            {
                if (EqualityComparer<TValue>.Default.Equals(current.Value, value))
                {
                    return index;
                }

                current = current.Next;
                index++;
            }

            return -1;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key, using the specified comparer.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="comparer">The equality comparer to use to determine whether the key exists.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the <see cref="ObservableDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValueWithKeyComparer(TKey key, IEqualityComparer<TKey> comparer, out TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            var current = Head;
            while (current != null)
            {
                if (comparer.Equals(current.Key, key) && current.Value != null)
                {
                    value = current.Value;
                    return true;
                }

                current = current.Next;
            }

            value = default!;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Inserts a new element with the specified key and value at the specified index in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the element should be inserted.</param>
    /// <param name="key">The key of the element to insert.</param>
    /// <param name="value">The value of the element to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.</exception>
    /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="ObservableDictionary{TKey, TValue}"/>.</exception>
    public void Insert(int index, TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists.");
            }

            if (index < 0 || index > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Node newNode = new Node(key, value);

            if (index == 0)
            {
                // Insert at the beginning
                newNode.Next = Head;
                if (Head != null)
                {
                    Head.Previous = newNode;
                }

                Head = newNode;
                Tail ??= newNode;
            }
            else if (index == _count)
            {
                // Insert at the end (same as Add)
                if (Tail != null)
                {
                    Tail.Next = newNode;
                    newNode.Previous = Tail;
                }

                Tail = newNode;
            }
            else
            {
                // Insert in the middle
                var current = Head;
                for (var i = 0; i < index - 1; i++)
                {
                    current = current?.Next;
                }

                newNode.Next = current?.Next;
                newNode.Previous = current;
                if (current != null)
                {
                    if (current.Next != null) current.Next.Previous = newNode;
                    current.Next = newNode;
                }
            }

            _count++;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                new KeyValuePair<TKey, TValue>(key, value), index));
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            _keys = null; // Invalidate cached keys
            _values = null; // Invalidate cached values
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Creates an <see cref="ObservableCollection{T}"/> from the key/value pairs in the <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>An <see cref="ObservableCollection{T}"/> containing the key/value pairs from the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
    public ObservableCollection<KeyValuePair<TKey, TValue>> ToObservableCollection()
    {
        _lock.EnterReadLock();
        try
        {
            return new ObservableCollection<KeyValuePair<TKey, TValue>>(this);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Helper class for IDictionaryEnumerator
    private class DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator) : IDictionaryEnumerator
    {
        public DictionaryEntry Entry => enumerator.Current.Key != null ? new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value) : default;

        public object Key => (enumerator.Current.Key ?? default)!;

        public object Value => enumerator.Current.Value ?? default!;

        public object Current => Entry;

        public bool MoveNext() => enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }
}

/*
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EGOIST.Domain.Structures;

public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary,
    INotifyCollectionChanged, INotifyPropertyChanged where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private bool _isInTransaction;
    private List<NotifyCollectionChangedEventArgs> _transactionChanges = [];
    private ICollection _keys => _dictionary.Keys;
    private ICollection _values => _dictionary.Values;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, TValue>(comparer);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
    }

    // IDictionary Implementation
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.TryGetValue(key, out var existingValue) &&
                !EqualityComparer<TValue>.Default.Equals(existingValue, value))
            {
                _dictionary[key] = value;
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, existingValue));
                OnPropertyChanged("Item[]");
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Values));
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public ICollection<TKey> Keys => _dictionary.Keys;

    ICollection IDictionary.Values => _values;

    ICollection IDictionary.Keys => _keys;

    public ICollection<TValue> Values => _dictionary.Values;

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public int Count => _dictionary.Count;
    public bool IsSynchronized { get; } = false;
    public object SyncRoot { get; } = new();
    public bool IsReadOnly => false;

    public object? this[object key]
    {
        get => _dictionary[(TKey)key];
        set
        {
            if (value == null)
                return;

            if (_dictionary.TryGetValue((TKey)key, out var existingValue) &&
                !EqualityComparer<TValue>.Default.Equals(existingValue, (TValue)value))
            {
                _dictionary[(TKey)key] = (TValue)value;
                OnCollectionChanged(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value),
                    new KeyValuePair<TKey, TValue>((TKey)key, existingValue));
                OnPropertyChanged("Item[]");
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Values));
            }
            else
                Add(key, value);
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (!_dictionary.TryAdd(key, value))
            _dictionary[key] = value;

        OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(Keys));
        OnPropertyChanged(nameof(Values));
        OnPropertyChanged("Item[]");
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Add(object key, object? value)
    {
        if (value == null)
            return;
        Add((TKey)key, (TValue)value);
    }

    public void Clear()
    {
        _dictionary.Clear();
        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
    }

    public bool Contains(object key)
    {
        return _dictionary.ContainsKey((TKey)key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public void Remove(object key)
    {
        if (!_dictionary.Remove((TKey)key)) return;

        OnCollectionChanged(NotifyCollectionChangedAction.Remove,
            new KeyValuePair<TKey, TValue>((TKey)key, _dictionary[(TKey)key]));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
    }

    public bool IsFixedSize { get; } = false;

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        if (!_dictionary.Remove(key, out var value)) return false;

        OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        return true;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!_dictionary.TryGetValue(item.Key, out var value) ||
            !EqualityComparer<TValue>.Default.Equals(value, item.Value)) return false;

        _dictionary.Remove(item.Key);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        return true;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _dictionary.TryGetValue(key, out value!);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Change Key
    public bool ChangeKey(TKey oldKey, TKey newKey)
    {
        if (!_dictionary.Remove(oldKey, out var value)) return false;

        _dictionary.Add(newKey, value);
        OnCollectionChanged(NotifyCollectionChangedAction.Replace,
            new KeyValuePair<TKey, TValue>(newKey, value),
            new KeyValuePair<TKey, TValue>(oldKey, value));
        OnPropertyChanged("Item[]");
        return true;
    }


    // Transactional Changes
    public void BeginTransaction()
    {
        _isInTransaction = true;
        _transactionChanges = [];
    }

    public void EndTransaction()
    {
        _isInTransaction = false;
        if (_transactionChanges.Count <= 0)
            return;

        OnCollectionChanged(NotifyCollectionChangedAction.Reset); // Single reset event for all changes
        _transactionChanges.Clear();
    }

    // Event Handlers (modified for transactions)
    private void OnCollectionChanged(NotifyCollectionChangedAction action)
    {
        if (_isInTransaction)
        {
            _transactionChanges.Add(new NotifyCollectionChangedEventArgs(action));
        }
        else
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        if (_isInTransaction)
        {
            _transactionChanges.Add(new NotifyCollectionChangedEventArgs(action, changedItem));
        }
        else
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem,
        KeyValuePair<TKey, TValue> oldItem)
    {
        if (_isInTransaction)
        {
            _transactionChanges.Add(new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }
        else
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
*/