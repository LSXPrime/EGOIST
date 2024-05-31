namespace EGOIST.Application.Utilities;

public static class Management
{
    /// <summary>
    /// Gets the model weight from a given name.
    /// </summary>
    /// <param name="name">The name to extract the weight from.</param>
    /// <returns>The extracted weight or "No match found" if no match is found.</returns>
    public static string GetModelWeight(this string name)
    {
        var weights = new[] { "FP32", "FP16", "Q8_0", "Q8_1", "Q8_K", "Q6_K", "Q5_0", "Q5_1", "Q5_K", "Q4_0", "Q4_1", "Q4_K", "Q3_K", "Q2_K", "IQ4_XS", "IQ3_S", "IQ3_XXS", "IQ2_XXS", "IQ2_S", "IQ2_XS", "IQ1_S", "IQ4_NL" };
        return weights.FirstOrDefault(name.Contains) ?? "No match found";
    }

    /// <summary>
    /// Gets the type and task from a Hugging Face pipeline tag.
    /// </summary>
    /// <param name="pipelineTag">The Hugging Face pipeline tag.</param>
    /// <returns>A tuple containing the type and task.</returns>
    public static (string Type, string Task) HuggingFaceGetTypeAndTask(this string pipelineTag)
    {
        return pipelineTag switch
        {
            "text-generation" or "translation" or "summarization" or "text2text-generation" => ("Text", "Generate"),
            "fill-mask" or "zero-shot-classification" or "text-classification" => ("Text", "Classify"),
            "question-answering" or "sentence-similarity" => ("Text", "Chat"),
            "feature-extraction" => ("Text", "Extract"),
            "image-classification" or "zero-shot-image-classification" => ("Image", "Classify"),
            "object-detection" or "zero-shot-object-detection" => ("Image", "Detect"),
            "image-segmentation" => ("Image", "Classify"),
            "text-to-image" or "image-to-image" => ("Image", "Generate"),
            "image-to-text" => ("Image", "Transcribe"),
            "automatic-speech-recognition" => ("Voice", "Transcribe"),
            "text-to-speech" or "audio-to-audio" => ("Voice", "Generate"),
            "audio-classification" => ("Voice", "Classify"),
            "visual-question-answering" or "image-text-to-text" => ("Image + Text", "Chat"),
            _ => ("Unknown", "Unknown")
        };
    }
}