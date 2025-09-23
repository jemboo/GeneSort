 using MessagePack;
using MessagePack.Resolvers;

namespace GeneSort.UI.Models
{
    // MessagePack configuration for F# interop
    public static class MessagePackConfig
    {
        private static MessagePackSerializerOptions? _options;

        public static MessagePackSerializerOptions Options
        {
            get
            {
                if (_options == null)
                {
                    // Create resolver that can handle F# types
                    var resolver = CompositeResolver.Create(
                        // Add FSharp resolver if you have MessagePack.FSharp package
                        MessagePack.FSharp.FSharpResolver.Instance,
                        StandardResolver.Instance
                    );

                    _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
                }
                return _options;
            }
        }
    }
}
