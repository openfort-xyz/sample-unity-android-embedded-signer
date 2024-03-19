using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(0)]
public class FirebaseManager : MonoBehaviour
{
    public event Action<FirebaseAuth, FirebaseFirestore> OnFirebaseInitialized;
    public static FirebaseManager Instance { get; private set; }
    protected string collectionPath = "";
    // DocumentID within the collection. Set to empty to use an autoid (which
    // obviously only works for writing new documents.)
    protected string documentId = "";
    protected string fieldContents;

    // Cancellation token source for the current operation.
    public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    // Previously completed task.
    public Task previousTask;
    // Whether an operation is in progress.
    public bool operationInProgress;

    public FirebaseAuth auth;
    public FirebaseFirestore db;

    [HideInInspector] public bool initialized;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async UniTaskVoid InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Setting up Firebase");
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;

            // Notify that Firebase has been initialized
            OnFirebaseInitialized?.Invoke(auth, db);
            initialized = true;
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            initialized = false;
        }
    }
    
    public IEnumerator UpdateDoc(DocumentReference doc, IDictionary<string, object> data)
    {
        Task updateTask = doc.UpdateAsync(data);
        yield return new WaitForTaskCompletion(this, updateTask);
        if (!(updateTask.IsFaulted || updateTask.IsCanceled))
        {
            // Update the collectionPath/documentId because:
            // 1) In the automated test, the caller might pass in an explicit docRef rather than pulling
            //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
            //    for the automated tests.)
            collectionPath = doc.Parent.Id;
            documentId = doc.Id;

            fieldContents = "Ok";
        }
        else
        {
            fieldContents = "Error";
        }
    }
    public IEnumerator WriteDoc(DocumentReference doc, IDictionary<string, object> data)
    {
        Task setTask = doc.SetAsync(data);
        yield return new WaitForTaskCompletion(this, setTask);
        if (!(setTask.IsFaulted || setTask.IsCanceled))
        {
            // Update the collectionPath/documentId because:
            // 1) If the documentId field was empty, this will fill it in with the autoid. This allows
            //    you to manually test via a trivial 'click set', 'click get'.
            // 2) In the automated test, the caller might pass in an explicit docRef rather than pulling
            //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
            //    for the automated tests.)
            collectionPath = doc.Parent.Id;
            documentId = doc.Id;

            fieldContents = "Ok";
        }
        else
        {
            fieldContents = "Error";
        }
    }

    // Wait for task completion, throwing an exception if the task fails.
    // This could be typically implemented using
    // yield return new WaitUntil(() => task.IsCompleted);
    // however, since many procedures in this sample nest coroutines and we want any task exceptions
    // to be thrown from the top level coroutine (e.g GetKnownValue) we provide this
    // CustomYieldInstruction implementation wait for a task in the context of the coroutine using
    // common setup and tear down code.
    class WaitForTaskCompletion : CustomYieldInstruction
    {
        Task task;
        FirebaseManager firebaseManager;

        // Create an enumerator that waits for the specified task to complete.
        public WaitForTaskCompletion(FirebaseManager firebaseManager, Task task)
        {
            firebaseManager.previousTask = task;
            firebaseManager.operationInProgress = true;
            this.firebaseManager = firebaseManager;
            this.task = task;
        }

        // Wait for the task to complete.
        public override bool keepWaiting
        {
            get
            {
                if (task.IsCompleted)
                {
                    firebaseManager.operationInProgress = false;
                    firebaseManager.cancellationTokenSource = new CancellationTokenSource();
                    if (task.IsFaulted)
                    {
                        string s = task.Exception.ToString();
                        Debug.Log(s);
                    }
                    return false;
                }
                return true;
            }
        }
    }

}
