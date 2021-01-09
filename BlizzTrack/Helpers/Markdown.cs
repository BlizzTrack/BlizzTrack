using Markdig;

namespace BlizzTrack.Helpers
{
    public static class Markdown
    {
        private static readonly MarkdownPipeline markdownPipeline = null;

        static Markdown()
        {
            var f = new MarkdownPipelineBuilder().UseAdvancedExtensions();
            f.Extensions.Add(new BootstrapExtension());

            markdownPipeline = f.Build();
        }

        public static string Parse(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown, markdownPipeline);
        }
    }
}