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
                switch (node)
                {
                    case Markdig.Extensions.Tables.Table:
                        node.GetAttributes().AddClass("table");
                        break;
                    case QuoteBlock:
                        node.GetAttributes().AddClass("blockquote");
                        break;
                    case Markdig.Extensions.Figures.Figure:
                        node.GetAttributes().AddClass("figure");
                        break;
                    case Block:
                    {
                        if (node is Markdig.Extensions.Figures.FigureCaption)
                        {
                            node.GetAttributes().AddClass("figure-caption");
                        }

                        break;
                    }
                    case Inline:
                    {
                        if (node is LinkInline {IsImage: true} link)
                        {
                            link.GetAttributes().AddClass("img-fluid");
                        }

                        break;
                    }
                }
            }
        }
    }
}