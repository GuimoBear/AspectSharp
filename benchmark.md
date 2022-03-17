``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  Job-ELFIZI : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  Job-CQFDCS : .NET 5.0.14 (5.0.1422.5710), X64 RyuJIT
  Job-QRLMQC : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT
  Job-BKFUHY : .NET Framework 4.8 (4.8.4470.0), X64 RyuJIT
  Job-JRFHQL : .NET Framework 4.8 (4.8.4470.0), X64 RyuJIT
  Job-NXXXOT : .NET Framework 4.8 (4.8.4470.0), X64 RyuJIT
  Job-QFTAQE : .NET Framework 4.8 (4.8.4470.0), X64 RyuJIT
  Job-YQZPNO : .NET Framework 4.8 (4.8.4470.0), X64 RyuJIT


```
|                   Schema |              Runtime |                        Method |       Mean |    StdDev |     Error |  Gen 0 |  Gen 1 | Allocated |
|------------------------- |--------------------- |------------------------------ |-----------:|----------:|----------:|-------:|-------:|----------:|
| AspectSharp interceptors |             .NET 6.0 |    CallUnmetrifiedFakeService |   352.6 ns |   1.74 ns |   1.57 ns | 0.0618 | 0.0202 |     392 B |
|                On method |             .NET 6.0 |    CallUnmetrifiedFakeService |   354.1 ns |   1.52 ns |   1.32 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 6.0 |               CallFakeService |   354.7 ns |   1.76 ns |   1.53 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 6.0 |    CallUnmetrifiedFakeService |   357.5 ns |   0.74 ns |   0.72 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 6.0 | CallFakeServiceWithoutMetrics |   361.1 ns |   1.20 ns |   1.04 ns | 0.0618 | 0.0202 |     392 B |
|                On method |             .NET 5.0 |    CallUnmetrifiedFakeService |   366.3 ns |   1.57 ns |   1.37 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 5.0 |    CallUnmetrifiedFakeService |   369.2 ns |   1.50 ns |   1.46 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 5.0 |               CallFakeService |   372.2 ns |   1.62 ns |   1.46 ns | 0.0618 | 0.0202 |     392 B |
|          Without metrics |             .NET 5.0 | CallFakeServiceWithoutMetrics |   374.1 ns |   3.49 ns |   3.14 ns | 0.0618 | 0.0202 |     392 B |
| AspectSharp interceptors |             .NET 5.0 |    CallUnmetrifiedFakeService |   378.3 ns |   0.61 ns |   0.53 ns | 0.0618 | 0.0202 |     392 B |
|                On method |             .NET 6.0 | CallFakeServiceWithoutMetrics |   413.9 ns |   3.94 ns |   3.54 ns | 0.0671 | 0.0221 |     424 B |
| AspectSharp interceptors |             .NET 6.0 | CallFakeServiceWithoutMetrics |   419.1 ns |   5.26 ns |   4.91 ns | 0.0710 | 0.0234 |     448 B |
|                On method |             .NET 5.0 | CallFakeServiceWithoutMetrics |   419.5 ns |   1.86 ns |   1.74 ns | 0.0671 | 0.0221 |     424 B |
|          Without metrics |        .NET Core 3.1 |    CallUnmetrifiedFakeService |   435.1 ns |   0.80 ns |   0.78 ns | 0.0612 | 0.0202 |     384 B |
|          Without metrics |        .NET Core 3.1 | CallFakeServiceWithoutMetrics |   435.5 ns |   2.36 ns |   2.41 ns | 0.0612 | 0.0202 |     384 B |
|          Without metrics |        .NET Core 3.1 |               CallFakeService |   436.1 ns |   2.19 ns |   2.05 ns | 0.0612 | 0.0202 |     384 B |
| AspectSharp interceptors |             .NET 5.0 | CallFakeServiceWithoutMetrics |   438.9 ns |   1.75 ns |   1.64 ns | 0.0710 | 0.0234 |     448 B |
|                On method |        .NET Core 3.1 |    CallUnmetrifiedFakeService |   440.1 ns |   1.74 ns |   1.62 ns | 0.0612 | 0.0202 |     384 B |
| AspectSharp interceptors |        .NET Core 3.1 |    CallUnmetrifiedFakeService |   456.6 ns |   2.13 ns |   1.85 ns | 0.0612 | 0.0202 |     384 B |
|                On method | .NET Framework 4.6.1 |    CallUnmetrifiedFakeService |   489.0 ns |   0.55 ns |   0.51 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics |   .NET Framework 4.7 | CallFakeServiceWithoutMetrics |   489.3 ns |   1.53 ns |   1.37 ns | 0.0618 | 0.0007 |     393 B |
|                On method |   .NET Framework 4.7 |    CallUnmetrifiedFakeService |   490.7 ns |   1.62 ns |   1.51 ns | 0.0618 | 0.0007 |     393 B |
|                On method |   .NET Framework 4.8 |    CallUnmetrifiedFakeService |   490.9 ns |   0.51 ns |   0.46 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.2 |    CallUnmetrifiedFakeService |   491.1 ns |   1.69 ns |   1.58 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics |   .NET Framework 4.7 |    CallUnmetrifiedFakeService |   491.5 ns |   1.93 ns |   1.88 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics |   .NET Framework 4.8 |               CallFakeService |   491.7 ns |   8.58 ns |   8.36 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.1 | CallFakeServiceWithoutMetrics |   491.8 ns |   0.99 ns |   0.89 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics |   .NET Framework 4.8 |    CallUnmetrifiedFakeService |   492.3 ns |   1.73 ns |   1.56 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics |   .NET Framework 4.8 | CallFakeServiceWithoutMetrics |   492.8 ns |   1.20 ns |   1.17 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.1 |               CallFakeService |   493.1 ns |   4.90 ns |   4.41 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.1 |    CallUnmetrifiedFakeService |   493.4 ns |   0.74 ns |   0.66 ns | 0.0618 | 0.0007 |     393 B |
|                On method |        .NET Core 3.1 | CallFakeServiceWithoutMetrics |   493.4 ns |   4.72 ns |   4.41 ns | 0.0658 | 0.0215 |     416 B |
|          Without metrics |   .NET Framework 4.7 |               CallFakeService |   493.6 ns |   5.42 ns |   5.28 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.2 | CallFakeServiceWithoutMetrics |   494.9 ns |   1.53 ns |   1.37 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.6.2 |               CallFakeService |   495.1 ns |   2.59 ns |   2.52 ns | 0.0618 | 0.0007 |     393 B |
|                On method | .NET Framework 4.6.2 |    CallUnmetrifiedFakeService |   498.3 ns |   1.00 ns |   0.87 ns | 0.0618 | 0.0007 |     393 B |
|                On method | .NET Framework 4.7.2 |    CallUnmetrifiedFakeService |   498.5 ns |   1.45 ns |   1.30 ns | 0.0618 | 0.0007 |     393 B |
| AspectSharp interceptors | .NET Framework 4.6.1 |    CallUnmetrifiedFakeService |   498.8 ns |   1.76 ns |   1.64 ns | 0.0618 | 0.0007 |     393 B |
| AspectSharp interceptors |        .NET Core 3.1 | CallFakeServiceWithoutMetrics |   502.8 ns |   5.66 ns |   5.09 ns | 0.0697 | 0.0228 |     440 B |
| AspectSharp interceptors | .NET Framework 4.7.2 |    CallUnmetrifiedFakeService |   503.3 ns |   4.41 ns |   4.30 ns | 0.0618 | 0.0007 |     393 B |
| AspectSharp interceptors |   .NET Framework 4.7 |    CallUnmetrifiedFakeService |   505.1 ns |   3.64 ns |   3.40 ns | 0.0618 | 0.0007 |     393 B |
| AspectSharp interceptors |   .NET Framework 4.8 |    CallUnmetrifiedFakeService |   507.0 ns |  10.53 ns |   9.15 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.7.2 |    CallUnmetrifiedFakeService |   509.2 ns |   1.15 ns |   1.04 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.7.2 | CallFakeServiceWithoutMetrics |   513.4 ns |   0.63 ns |   0.59 ns | 0.0618 | 0.0007 |     393 B |
| AspectSharp interceptors | .NET Framework 4.6.2 |    CallUnmetrifiedFakeService |   513.9 ns |   6.50 ns |   5.85 ns | 0.0618 | 0.0007 |     393 B |
|          Without metrics | .NET Framework 4.7.2 |               CallFakeService |   519.7 ns |   2.19 ns |   2.13 ns | 0.0618 | 0.0007 |     393 B |
|                On method | .NET Framework 4.6.2 | CallFakeServiceWithoutMetrics |   569.1 ns |   3.20 ns |   3.12 ns | 0.0671 | 0.0007 |     425 B |
|                On method | .NET Framework 4.6.1 | CallFakeServiceWithoutMetrics |   570.5 ns |   5.58 ns |   5.21 ns | 0.0671 | 0.0007 |     425 B |
|                On method |   .NET Framework 4.7 | CallFakeServiceWithoutMetrics |   572.0 ns |   9.07 ns |   7.88 ns | 0.0671 | 0.0007 |     425 B |
|                On method |   .NET Framework 4.8 | CallFakeServiceWithoutMetrics |   573.6 ns |   7.49 ns |   6.74 ns | 0.0671 | 0.0007 |     425 B |
|                On method | .NET Framework 4.7.2 | CallFakeServiceWithoutMetrics |   574.4 ns |   6.06 ns |   5.26 ns | 0.0671 | 0.0007 |     425 B |
| AspectSharp interceptors |   .NET Framework 4.7 | CallFakeServiceWithoutMetrics |   588.2 ns |   1.99 ns |   1.79 ns | 0.0710 | 0.0007 |     449 B |
| AspectSharp interceptors | .NET Framework 4.7.2 | CallFakeServiceWithoutMetrics |   590.2 ns |   0.75 ns |   0.76 ns | 0.0710 | 0.0007 |     449 B |
| AspectSharp interceptors |   .NET Framework 4.8 | CallFakeServiceWithoutMetrics |   590.6 ns |   7.59 ns |   6.59 ns | 0.0710 | 0.0007 |     449 B |
| AspectSharp interceptors | .NET Framework 4.6.2 | CallFakeServiceWithoutMetrics |   591.4 ns |   1.63 ns |   1.47 ns | 0.0710 | 0.0007 |     449 B |
| AspectSharp interceptors | .NET Framework 4.6.1 | CallFakeServiceWithoutMetrics |   592.5 ns |   2.36 ns |   2.12 ns | 0.0710 | 0.0007 |     449 B |
|                On method |             .NET 6.0 |               CallFakeService |   688.4 ns |   9.45 ns |   8.50 ns | 0.1159 | 0.0378 |     728 B |
|                On method |             .NET 5.0 |               CallFakeService |   713.6 ns |   2.26 ns |   2.04 ns | 0.1159 | 0.0378 |     728 B |
|                On method |        .NET Core 3.1 |               CallFakeService |   867.6 ns |   1.64 ns |   1.47 ns | 0.1146 | 0.0378 |     720 B |
| AspectSharp interceptors |             .NET 6.0 |               CallFakeService |   931.9 ns |   2.27 ns |   2.13 ns | 0.1602 | 0.0534 |   1,008 B |
|                On method |   .NET Framework 4.8 |               CallFakeService |   956.0 ns |   1.17 ns |   1.05 ns | 0.1172 | 0.0013 |     738 B |
|                On method |   .NET Framework 4.7 |               CallFakeService |   958.2 ns |   1.20 ns |   1.16 ns | 0.1172 | 0.0013 |     738 B |
|                On method | .NET Framework 4.7.2 |               CallFakeService |   962.7 ns |   2.44 ns |   2.48 ns | 0.1172 | 0.0013 |     738 B |
|                On method | .NET Framework 4.6.1 |               CallFakeService |   965.6 ns |   1.36 ns |   1.27 ns | 0.1172 | 0.0013 |     738 B |
|                On method | .NET Framework 4.6.2 |               CallFakeService |   966.0 ns |   1.47 ns |   1.38 ns | 0.1172 | 0.0013 |     738 B |
| AspectSharp interceptors |             .NET 5.0 |               CallFakeService |   969.7 ns |   3.20 ns |   2.99 ns | 0.1602 | 0.0534 |   1,008 B |
| AspectSharp interceptors |        .NET Core 3.1 |               CallFakeService | 1,220.8 ns |   4.65 ns |   4.04 ns | 0.1576 | 0.0521 |     992 B |
| AspectSharp interceptors |   .NET Framework 4.7 |               CallFakeService | 1,370.2 ns |   1.89 ns |   1.84 ns | 0.1589 | 0.0026 |   1,011 B |
| AspectSharp interceptors |   .NET Framework 4.8 |               CallFakeService | 1,396.6 ns |   1.61 ns |   1.57 ns | 0.1589 | 0.0026 |   1,011 B |
| AspectSharp interceptors | .NET Framework 4.6.2 |               CallFakeService | 1,400.7 ns |   1.66 ns |   1.50 ns | 0.1589 | 0.0026 |   1,011 B |
| AspectSharp interceptors | .NET Framework 4.7.2 |               CallFakeService | 1,408.0 ns |   4.52 ns |   4.07 ns | 0.1589 | 0.0026 |   1,011 B |
| AspectSharp interceptors | .NET Framework 4.6.1 |               CallFakeService | 1,412.2 ns |   1.67 ns |   1.63 ns | 0.1589 | 0.0026 |   1,011 B |
|  AspectCore interceptors |             .NET 6.0 |    CallUnmetrifiedFakeService | 2,309.7 ns |  27.14 ns |  25.36 ns | 0.4063 | 0.1328 |   2,560 B |
|  AspectCore interceptors |             .NET 5.0 |    CallUnmetrifiedFakeService | 2,923.4 ns |  33.01 ns |  30.85 ns | 0.4271 | 0.1042 |   2,688 B |
|  AspectCore interceptors |        .NET Core 3.1 |    CallUnmetrifiedFakeService | 3,365.0 ns |  25.44 ns |  22.89 ns | 0.4271 |      - |   2,696 B |
|  AspectCore interceptors |             .NET 6.0 | CallFakeServiceWithoutMetrics | 3,694.7 ns |  17.73 ns |  15.95 ns | 0.5938 | 0.1458 |   3,736 B |
|  AspectCore interceptors |             .NET 5.0 | CallFakeServiceWithoutMetrics | 4,072.9 ns |  17.99 ns |  15.62 ns | 0.6146 | 0.1510 |   3,864 B |
|  AspectCore interceptors |   .NET Framework 4.7 |    CallUnmetrifiedFakeService | 4,277.6 ns |   6.14 ns |   5.98 ns | 0.4323 | 0.1042 |   2,744 B |
|  AspectCore interceptors | .NET Framework 4.6.1 |    CallUnmetrifiedFakeService | 4,349.9 ns |  16.45 ns |  15.38 ns | 0.4323 | 0.1042 |   2,744 B |
|  AspectCore interceptors |   .NET Framework 4.8 |    CallUnmetrifiedFakeService | 4,366.1 ns |  11.18 ns |  10.06 ns | 0.4323 | 0.1042 |   2,744 B |
|  AspectCore interceptors | .NET Framework 4.7.2 |    CallUnmetrifiedFakeService | 4,369.3 ns |  14.01 ns |  12.61 ns | 0.4323 | 0.1042 |   2,744 B |
|  AspectCore interceptors | .NET Framework 4.6.2 |    CallUnmetrifiedFakeService | 4,374.0 ns |  15.24 ns |  14.84 ns | 0.4323 | 0.1042 |   2,744 B |
|  AspectCore interceptors |        .NET Core 3.1 | CallFakeServiceWithoutMetrics | 4,729.4 ns |  20.38 ns |  19.05 ns | 0.6146 |      - |   3,872 B |
|  AspectCore interceptors |             .NET 6.0 |               CallFakeService | 5,177.5 ns |  18.75 ns |  16.87 ns | 0.7448 | 0.1458 |   4,696 B |
|  AspectCore interceptors |   .NET Framework 4.7 | CallFakeServiceWithoutMetrics | 5,914.9 ns |  13.67 ns |  11.87 ns | 0.6146 |      - |   3,924 B |
|  AspectCore interceptors | .NET Framework 4.6.1 | CallFakeServiceWithoutMetrics | 5,923.2 ns |  33.76 ns |  30.38 ns | 0.6146 |      - |   3,924 B |
|  AspectCore interceptors |             .NET 5.0 |               CallFakeService | 5,933.9 ns |  30.02 ns |  29.23 ns | 0.7604 | 0.1458 |   4,824 B |
|  AspectCore interceptors |   .NET Framework 4.8 | CallFakeServiceWithoutMetrics | 5,955.2 ns |  11.75 ns |  11.44 ns | 0.6146 |      - |   3,924 B |
|  AspectCore interceptors | .NET Framework 4.6.2 | CallFakeServiceWithoutMetrics | 5,991.5 ns |  15.88 ns |  16.17 ns | 0.6146 |      - |   3,924 B |
|  AspectCore interceptors | .NET Framework 4.7.2 | CallFakeServiceWithoutMetrics | 6,135.3 ns |  30.42 ns |  28.43 ns | 0.6146 |      - |   3,924 B |
|  AspectCore interceptors |        .NET Core 3.1 |               CallFakeService | 7,066.5 ns |  58.52 ns |  56.99 ns | 0.7604 |      - |   4,832 B |
|  AspectCore interceptors |   .NET Framework 4.8 |               CallFakeService | 8,659.1 ns |  55.20 ns |  56.20 ns | 0.7708 |      - |   4,886 B |
|  AspectCore interceptors | .NET Framework 4.6.1 |               CallFakeService | 8,688.2 ns | 131.12 ns | 122.55 ns | 0.7708 |      - |   4,886 B |
|  AspectCore interceptors | .NET Framework 4.7.2 |               CallFakeService | 8,868.3 ns | 161.62 ns | 145.40 ns | 0.7708 |      - |   4,886 B |
|  AspectCore interceptors |   .NET Framework 4.7 |               CallFakeService | 8,898.8 ns | 190.64 ns | 171.52 ns | 0.7708 |      - |   4,886 B |
|  AspectCore interceptors | .NET Framework 4.6.2 |               CallFakeService | 9,040.5 ns | 213.45 ns | 192.04 ns | 0.7708 |      - |   4,886 B |
