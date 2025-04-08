using Cysharp.Threading.Tasks;

namespace Codebase.Structure
{
    public class GameModule
    {
        public readonly UniTaskCompletionSource<bool> CompletionSource = new();
    }

    public class GameModule<TResult> : GameModule
    {
        public TResult Result;

        public void Finish(TResult result)
        {
            Result = result;
            CompletionSource.TrySetResult(true);
        }
    }
}