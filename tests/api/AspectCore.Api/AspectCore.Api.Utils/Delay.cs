namespace AspectCore.Api.Utils
{
    public static class Delay
    {
        public static async Task Random(int minMiliseconds, int maxMiliseconds)
            => await Task.Delay(TimeSpan.FromMilliseconds(System.Random.Shared.Next(minMiliseconds, maxMiliseconds) / 100.0));

        public static Task Random()
            => Random(1, 11);
    }
}
