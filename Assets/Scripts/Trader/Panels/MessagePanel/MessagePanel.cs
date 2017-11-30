using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour {

    private const float MessageQueueDelay = 2f;
    private const float MessageQueueFlushDelay = 0.1f;
    private const string MessageSeparator = "#####################";

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform messafeItemContainer;
    [SerializeField] private MessageItem messageItemPrefab;

    private bool isDisplayingMessageQueue = false;
    private Queue<string> messageQueue;
    private string messageQueueType;

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Space)) {
            if (isDisplayingMessageQueue && messageQueue.Count > 0) {
                ForceNextMessageInQueue();
            }
        }
    }

    public void DisplayMessage(string type, string message) {
        MessageItem item = Instantiate(messageItemPrefab, messafeItemContainer, false);
        item.Setup(type, message);
        ScrollPanelToBottom();
    }

    private void ScrollPanelToBottom() {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    public void StartMessageQueue(string type, string[] messages, bool addSeparator = false, bool delayFirst = false) {
        if (isDisplayingMessageQueue) {
            FlushCurrentQueueAndStartNext(type, messages, addSeparator, delayFirst);
        }
        else {
            isDisplayingMessageQueue = true;
            messageQueueType = type;
            messageQueue = new Queue<string>(messages);
            if (addSeparator) {
                DisplayMessage(messageQueueType, MessageSeparator);
            }
            if (!delayFirst) {
                DisplayMessage(messageQueueType, messageQueue.Dequeue()); // shows first message without delay, removing from queue
            }
            StartCoroutine(DisplayMessageQueue());
        }
    }

    private void FlushCurrentQueueAndStartNext(string nextType, string[] nextMessages, bool addSeparator, bool delayFirst) {
        StopAllCoroutines();
        StartCoroutine(FlushMessageQueue(nextType, nextMessages, addSeparator, delayFirst));
    }

    private void ResumeMessageQueue() {
        StartCoroutine(DisplayMessageQueue());
    }

    private void ForceNextMessageInQueue() {
        StopAllCoroutines();
        string nextMessage = messageQueue.Dequeue();
        DisplayMessage(messageQueueType, nextMessage);
        ResumeMessageQueue();
    }

    private IEnumerator DisplayMessageQueue() {
        while (messageQueue.Count > 0) {
            yield return new WaitForSeconds(MessageQueueDelay);
            DisplayMessage(messageQueueType, messageQueue.Dequeue());
        }
        HandleQueueFinished();
    }

    private void HandleQueueFinished() {
        CleanupMessageQueue();
    }

    private IEnumerator FlushMessageQueue(string nextType, string[] nextMessages, bool addSeparator, bool delayFirst) {
        while (messageQueue.Count > 0) {
            yield return new WaitForSeconds(MessageQueueFlushDelay);
            DisplayMessage(messageQueueType, messageQueue.Dequeue());
        }
        HandleFlushQueueFinished(nextType, nextMessages, addSeparator, delayFirst);
    }

    private void HandleFlushQueueFinished(string nextType, string[] nextMessages, bool addSeparator, bool delayFirst) {
        isDisplayingMessageQueue = false;
        StartMessageQueue(nextType, nextMessages, addSeparator, delayFirst);
    }

    private void CleanupMessageQueue() {
        isDisplayingMessageQueue = false;
        messageQueueType = null;
        messageQueue = null;
    }

}
