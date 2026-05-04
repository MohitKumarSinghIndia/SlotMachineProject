using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SlotFlowController : MonoBehaviour
    {
        private readonly Queue<Func<IEnumerator>> spinStartQueue = new();
        private readonly Queue<Func<IEnumerator>> spinStopQueue = new();
        private readonly Queue<Func<IEnumerator>> resultDisplayQueue = new();
        private readonly Queue<Func<IEnumerator>> freeGameQueue = new();
        private readonly Queue<Func<IEnumerator>> bigWinQueue = new();

        private bool isRunning;

        public bool IsRunning => isRunning;

        public void StartSpinFlow()
        {
            if (isRunning)
            {
                return;
            }

            StartCoroutine(RunSpinFlow());
        }

        public void AddSpinStartStep(Func<IEnumerator> step)
        {
            AddStep(spinStartQueue, step);
        }

        public void AddSpinStopStep(Func<IEnumerator> step)
        {
            AddStep(spinStopQueue, step);
        }

        public void AddResultDisplayStep(Func<IEnumerator> step)
        {
            AddStep(resultDisplayQueue, step);
        }

        public void AddBigWinStep(Func<IEnumerator> step)
        {
            AddStep(bigWinQueue, step);
        }

        public void AddFreeGameStep(Func<IEnumerator> step)
        {
            AddStep(freeGameQueue, step);
        }

        public void ClearAllQueues()
        {
            if (isRunning)
            {
                return;
            }

            spinStartQueue.Clear();
            spinStopQueue.Clear();
            resultDisplayQueue.Clear();
            freeGameQueue.Clear();
            bigWinQueue.Clear();
        }

        private void AddStep(Queue<Func<IEnumerator>> queue, Func<IEnumerator> step)
        {
            if (step == null)
            {
                return;
            }

            queue.Enqueue(step);
        }

        private IEnumerator RunSpinFlow()
        {
            isRunning = true;

            try
            {
                yield return RunQueue(spinStartQueue);
                yield return RunQueue(spinStopQueue);
                yield return RunQueue(resultDisplayQueue);
                yield return RunQueue(freeGameQueue);
                yield return RunQueue(bigWinQueue);
            }
            finally
            {
                isRunning = false;
            }
        }

        private IEnumerator RunQueue(Queue<Func<IEnumerator>> queue)
        {
            while (queue.Count > 0)
            {
                Func<IEnumerator> step = queue.Dequeue();
                if (step == null)
                {
                    continue;
                }

                IEnumerator routine = step();
                if (routine != null)
                {
                    yield return StartCoroutine(routine);
                }
            }
        }
    }
}
