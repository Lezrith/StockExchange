using System;

namespace Logic
{
    public interface IManager
    {
        void Start(int numberOfThreads);

        void Wait();
    }
}
