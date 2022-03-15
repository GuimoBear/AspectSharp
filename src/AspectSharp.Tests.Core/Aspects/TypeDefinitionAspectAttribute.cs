using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Aspects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class TypeDefinitionAspect1Attribute : AbstractInterceptorAttribute
    {
        private static readonly Random _random = new Random();

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine("{0}: {1}.{2}", nameof(TypeDefinitionAspect1Attribute), context.TargetType.Name, context.TargetMethod.Name);
            context.AdditionalData.Add(nameof(TypeDefinitionAspect1Attribute), _random.Next());
            await next(context);
        }
    }
}
