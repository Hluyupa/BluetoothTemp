using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public class QueueController<TObjectsOfQueue>
    {
        Queue<TObjectsOfQueue> Queue;

        private readonly Action<TObjectsOfQueue> QueueStartEvent;
        private readonly Action<TObjectsOfQueue> EveryStepEvent;
        private readonly Action QueueEndedEvent;
        public bool IsQueueStarted { get; private set; }
        public QueueController(Queue<TObjectsOfQueue> queue, Action<TObjectsOfQueue> queueStartEvent, Action<TObjectsOfQueue> everyStepEvent, Action queueEndedEvent = null)
        {
            IsQueueStarted = false;
            Queue = queue;
            QueueStartEvent = queueStartEvent;
            EveryStepEvent = everyStepEvent;
            QueueEndedEvent = queueEndedEvent;
        }
        public void StartQueue()
        {
            if (!IsQueueStarted)
            {
                IsQueueStarted = true;
                QueueStartEvent?.Invoke(Queue.Dequeue());
            }
            else
            {
                return;
            }
            
           
        }
        public void NextAction()
        {
            if (IsQueueStarted && Queue.Count > 0)
            {
                EveryStepEvent?.Invoke(Queue.Dequeue());
            }
            else if (Queue.Count == 0)
            {
                IsQueueStarted = false;
                QueueEndedEvent?.Invoke();
            }
        }
    }
}
