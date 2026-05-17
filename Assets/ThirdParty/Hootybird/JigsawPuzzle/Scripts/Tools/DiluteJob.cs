using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace HootyBird.JigsawPuzzleEngine.Tools
{
    [BurstCompile(
        CompileSynchronously = true,
        FloatMode = FloatMode.Fast,
        FloatPrecision = FloatPrecision.Low,
        OptimizeFor = OptimizeFor.Performance)]
    public struct DiluteJob : IJob
    {
        [ReadOnly]
        public NativeArray<byte> input;
        [ReadOnly]
        public NativeReference<int2> textureSize;
        [ReadOnly]
        public NativeReference<int> dilutePower;

        public NativeArray<byte> output;

        public void Execute()
        {
            int index;
            int targetIndex;
            int2 from;
            int2 to;
            int red;
            int sideSize = dilutePower.Value + dilutePower.Value + 1;
            int pixelsToCheck = sideSize * sideSize - 1;
            float step = 1f / pixelsToCheck;
            for (int x = 0; x < textureSize.Value.x; x++)
            {
                for (int y = 0; y < textureSize.Value.y; y++)
                {
                    index = y * textureSize.Value.x + x;

                    // Red channel == number of pixels around this pixels.
                    if (input[index] > 0)
                    {
                        // Scan around.
                        from = new int2(math.max(
                            0, x - dilutePower.Value), 
                            math.max(0, y - dilutePower.Value));
                        to = new int2(
                            math.min(textureSize.Value.x, x + dilutePower.Value), 
                            math.min(textureSize.Value.y, y + dilutePower.Value));
                        red = 0;
                        for (int _x = from.x; _x <= to.x; _x++)
                        {
                            for (int _y = from.y; _y <= to.y; _y++)
                            {
                                // Skip self.
                                if (_x == x && _y == y)
                                {
                                    continue;
                                }

                                targetIndex = _y * textureSize.Value.x + _x;
                                red += input[targetIndex];
                            }
                        }
                        output[index] = (byte)(red * step * (input[index] / 255f));
                    }
                }
            }
        }

        public void Init()
        {
            output = new NativeArray<byte>(textureSize.Value.x * textureSize.Value.y, Allocator.TempJob);
        }

        public void Dispose()
        {
            input.Dispose();
            textureSize.Dispose();
            output.Dispose();
            dilutePower.Dispose();
        }
    }
}
