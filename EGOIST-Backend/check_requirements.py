import subprocess

def install_from_pytorch_index(package):
    subprocess.run(['pip', 'install', package, '--index-url', 'https://download.pytorch.org/whl/cu118'])

def check_requirements():
    with open('requirements.txt', 'r') as file:
        packages = file.readlines()

    for package in packages:
        package = package.strip()
        try:
            subprocess.run(['pip', 'show', package], check=True)
            print(f"{package}: OK")
        except subprocess.CalledProcessError:
            print(f"{package}: Not found in venv. Installing...")
            if package in ['torch', 'torchvision', 'torchaudio']:
                install_from_pytorch_index(package)
            else:
                subprocess.run(['pip', 'install', package])

    print("Installation Complete")

if __name__ == "__main__":
    check_requirements()
