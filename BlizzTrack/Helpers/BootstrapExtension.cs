using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BlizzTrack.Helpers
{
    public class BootstrapExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Make sure we don't have a delegate twice
            pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
            pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }

        private static void PipelineOnDocumentProcessed(MarkdownDocument document)
        {
            foreach (var node in document.Descendants())
            {
                if (node is Block)
                {
                    if (node is Markdig.Extensions.Tables.Table)
                    {
                        node.GetAttributes().AddClass("table");
                    }
                    else if (node is QuoteBlock)
                    {
                        node.GetAttributes().AddClass("blockquote");
                    }
                    else if (node is Markdig.Extensions.Figures.Figure)
                    {
                        node.GetAttributes().AddClass("figure");
                    }
                    else if (node is Markdig.Extensions.Figures.FigureCaption)
                    {
                        node.GetAttributes().AddClass("figure-caption");
                    }
                }
                else if (node is Inline)
                {
                    if (node is LinkInline link && link.IsImage)
                    {
                        link.GetAttributes().AddClass("img-fluid");
                    }
                }
            }
        }
    }
}