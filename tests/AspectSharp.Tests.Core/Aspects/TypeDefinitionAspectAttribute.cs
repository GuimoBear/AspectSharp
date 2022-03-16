﻿using AspectSharp.Abstractions;
using AspectSharp.Abstractions.Attributes;
using AspectSharp.Tests.Core.Enums;
using System;
using System.Threading.Tasks;

namespace AspectSharp.Tests.Core.Aspects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class TypeDefinitionAspectAttribute : AbstractInterceptorAttribute
    {
        private static readonly string _aspectName = nameof(TypeDefinitionAspectAttribute).PadRight(9);

        private static readonly Random _random = new Random();

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            context.AdditionalData.Add(string.Format("{0}: {1} {2}", _aspectName, InterceptMoment.Before.ToString().ToLower(), context.ServiceMethod.Name), _random.Next());
            await next(context);
            context.AdditionalData.Add(string.Format("{0}: {1} {2}", _aspectName, InterceptMoment.After.ToString().ToLower(), context.ServiceMethod.Name), _random.Next());
        }
    }
}
