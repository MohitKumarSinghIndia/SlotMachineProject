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
        private readonly Queue<Func<IEnumerator>> paylineQueue = new();
        private readonly Queue<Func<IEnumerator>> bigWinQueue = new();
        private readonly Queue<Func<IEnumerator>> freeGameQueue = new();
        private readonly Queue<Func<IEnumerator>> completeQueue = new();

        private bool isRunning;

        public bool IsRunning => isRunning;

        public event Action SpinFlowStarted;
        public event Action SpinStartPhaseStarted;
        public event Action SpinStopPhaseStarted;
        public event Action ResultDisplayPhaseStarted;
        public event Action LineWinPhaseStarted;
        public event Action BigWinPhaseStarted;
        public event Action FreeGamePhaseStarted;
        public event Action SpinFlowCompleted;

        public void StartSpinFlow()
        {
            if (isRunning)
            {
                return;
            }

            StartCoroutine(RunSpinFlow());
        }

        public void AddSpinStartStep(Func<IEnumerator> step) => AddStep(spinStartQueue, step);
        public void AddSpinStopStep(Func<IEnumerator> step) => AddStep(spinStopQueue, step);
        public void AddResultDisplayStep(Func<IEnumerator> step) => AddStep(resultDisplayQueue, step);
        public void AddLineWinStep(Func<IEnumerator> step) => AddStep(paylineQueue, step);
        public void AddBigWinStep(Func<IEnumerator> step) => AddStep(bigWinQueue, step);
        public void AddFreeGameStep(Func<IEnumerator> step) => AddStep(freeGameQueue, step);
        public void AddCompleteStep(Func<IEnumerator> step) => AddStep(completeQueue, step);


        

        public void ClearAllQueues()
        {
            if (isRunning)
            {
                return;
            }

            spinStartQueue.Clear();
            spinStopQueue.Clear();
            resultDisplayQueue.Clear();
            paylineQueue.Clear();
            bigWinQueue.Clear();
            freeGameQueue.Clear();
            completeQueue.Clear();
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
            SpinFlowStarted?.Invoke();

            yield return RunNamedQueue(SpinStartPhaseStarted, spinStartQueue);
            yield return RunNamedQueue(SpinStopPhaseStarted, spinStopQueue);
            yield return RunNamedQueue(ResultDisplayPhaseStarted, resultDisplayQueue);
            yield return RunNamedQueue(LineWinPhaseStarted, paylineQueue);
            yield return RunNamedQueue(BigWinPhaseStarted, bigWinQueue);
            yield return RunNamedQueue(FreeGamePhaseStarted, freeGameQueue);
            yield return RunQueue(completeQueue);

            isRunning = false;
            SpinFlowCompleted?.Invoke();
        }

        private IEnumerator RunNamedQueue(Action phaseStarted, Queue<Func<IEnumerator>> queue)
        {
            if (queue.Count <= 0)
            {
                yield break;
            }

            phaseStarted?.Invoke();
            yield return RunQueue(queue);
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
