﻿namespace AspectSharp.Benchmarks.Services.Interfaces
{
    public interface IAnotherFakeService
    {
        string SayHello(string param);
        string SayHelloWithoutMetrics(string param);
    }
}