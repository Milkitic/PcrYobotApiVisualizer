using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace YamlDotNet.Comment
{
    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            var commentsDescriptor = value as CommentsObjectDescriptor;
            if (commentsDescriptor != null && commentsDescriptor.Comment != null)
            {
                context.Emit(new Core.Events.Comment(commentsDescriptor.Comment, false));
            }

            return base.EnterMapping(key, value, context);
        }
    }
}