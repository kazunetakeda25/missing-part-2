using System;
using System.Collections.Generic;
using UnityEngine;
using ScreenFaderComponents;
using ScreenFaderComponents.Actions;
using ScreenFaderComponents.Enumerators;
using ScreenFaderComponents.Events;

public class Fader : MonoBehaviour, IFader
{
#region Inspector properties

    [SerializeField]
    private FadeDirection defaultState = FadeDirection.Out;
    /// <summary>
    /// Delay before fade-in starts
    /// </summary>
    [Range(0, 10)]
    public float fadeInDelay;
    /// <summary>
    /// Delay before fade-out starts
    /// </summary>
    [Range(0, 10)]
    public float fadeOutDelay;
    /// <summary>
    /// GUI.depth
    /// </summary>
    [SerializeField]
    [Range(-5, 5)]
    private int GUIdepth = -2;

#endregion

#region Chains methods

    /// <summary>
    /// Customisable fading
    /// </summary>
    /// <param name="fadeDirection">FadeDirection</param>
    /// <param name="time">Fading speed (time of fading), default value is 1 second</param>
    public IFader Fade(FadeDirection fadeDirection, float time = 1)
    {
        AddTask(new FaderTask()
        {
            State = fadeDirection == FadeDirection.In ? FadeState.In : FadeState.Out,
            Time = time
        });
        return Instance;
    }
    /// <summary>
    /// Fade in screen
    /// </summary>
    /// <param name="time">Fading speed (time of fading), default value is 1 second</param>
    public IFader FadeIn(float time = 1)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.In,
            Time = time
        });
        return Instance;
    }
    /// <summary>
    /// Fade out screen
    /// </summary>
    /// <param name="time">Fading speed (time of fading), default value is 1 second</param>
    public IFader FadeOut(float time = 1)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.Out,
            Time = time
        });
        return Instance;
    }
    /// <summary>
    /// Pause in the chain
    /// </summary>
    /// <param name="pause">Pause delay, default value is 1 second</param>
    public IFader Pause(float pause = 1)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.Stop,
            PostDelay = pause
        });
        return Instance;
    }

    /// <summary>
    /// Add any action to the fadings chain. 
    /// Implement IAction interface to create any action than you want, it can be changing of player character or location, switching of cameras or anything else.
    /// </summary>
    public IFader StartAction(IAction action)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.Stop,
            action = action
        });
        return Instance;
    }
    /// <summary>
    /// Add any action to the fadings chain. 
    /// Implement IAction interface to create any action than you want, it can be changing of player character or location, switching of cameras or anything else.
    /// </summary>
    public IFader StartAction(IParametrizedAction action, params object[] args)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.Stop,
            pAction = action,
            pActionParameters = args
        });
        return Instance;
    }

    /// <summary>
    /// Flash effect - quick fade-in, and fade out
    /// </summary>
    public IFader Flash(float inTime = 0.075f, float outTime = 0.15f)
    {
        AddTask(new FaderTask()
        {
            State = FadeState.In,
            Time = inTime,
            PostDelay = 0.1f
        });
        AddTask(new FaderTask()
        {
            State = FadeState.Out,
            Time = outTime,
        });
        return Instance;
    }

    public void StopAllFadings()
    {
        tasks.Clear();
    }

    public IFader StartCoroutine(MonoBehaviour component, string methodName)
    {
        if (component == null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException();

        return StartAction(new CoroutineAction(), component, methodName);
    }
    public IFader StartCoroutine(MonoBehaviour component, string methodName, object value)
    {
        if (component == null) throw new ArgumentNullException();
        if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException();

        return StartAction(new ParametrizedCoroutineAction(), component, methodName, value);
    }
    public IFader StartCoroutine(MonoBehaviour component, System.Collections.IEnumerator routine)
    {
        if (component == null) throw new ArgumentNullException();
        if (routine == null) throw new ArgumentNullException();

        return StartAction(new EnumeratorCoroutineAction(), component, routine);
    }
    public static IFader Fade(FadeDirection fadeDirection, float time, IAction action, float postDelay)
    {
        Instance.AddTask(new FaderTask() {
            State = fadeDirection == FadeDirection.In ? FadeState.In : FadeState.Out,
            Time = time,
            PostDelay = postDelay,
            action = action
        });

        return Instance;
    }

#endregion

    protected float fadeBalance;
    protected FadeState currentState = FadeState.OutEnd;
    public FadeState State { get { return currentState; } }
    protected FaderTask currentTask = null;
    protected Queue<FaderTask> tasks = new Queue<FaderTask>();

    /// <summary>
    /// Add fading tast to the queue
    /// </summary>
    /// <param name="task"></param>
    public void AddTask(FaderTask task)
    {
        tasks.Enqueue(task);
    }
    /// <summary>
    /// Start first task from queue, remove it from queue and raise FadeStart event
    /// </summary>
    protected void StartTask()
    {
        if (currentTask == null && tasks.Count > 0)
        {
            currentTask = tasks.Dequeue();
            if (currentTask.action != null)
                currentTask.action.Completed = false;
            if (currentTask.pAction != null)
                currentTask.pAction.Completed = false;

            OnFadeStart(new FadeEventArgs() { Direction = currentTask.State == FadeState.In ? FadeDirection.In : FadeDirection.Out });
        }
    }
    /// <summary>
    /// Executes an action if it exists
    /// </summary>
    protected void ExecuteTaskAction()
    {
        if (currentTask.action != null && !currentTask.action.Completed)
            currentTask.action.Execute();
        if (currentTask.pAction != null && !currentTask.pAction.Completed)
            currentTask.pAction.Execute(currentTask.pActionParameters);
    }
    /// <summary>
    /// Called when fading task finished, call task's action and raise FadeFinish event
    /// </summary>
    protected void FinishTask()
    {
        if (currentTask != null)
        {
            if (currentTask.action == null & currentTask.pAction == null)
            {
                OnFadeFinish(new FadeEventArgs() { Direction = currentTask.State == FadeState.In ? FadeDirection.In : FadeDirection.Out });
                currentTask = null;
            }
            else
            {
                if (currentTask.action != null)
                {
                    if (currentTask.action.Completed)
                    {
                        OnFadeFinish(new FadeEventArgs() { Direction = currentTask.State == FadeState.In ? FadeDirection.In : FadeDirection.Out });
                        currentTask = null;
                    }
                }
                else if (currentTask.pAction != null)
                {
                    if (currentTask.pAction.Completed)
                    {
                        OnFadeFinish(new FadeEventArgs() { Direction = currentTask.State == FadeState.In ? FadeDirection.In : FadeDirection.Out });
                        currentTask = null;
                    }
                }
            }
        }
    }
    /// <summary>
    /// This event occurs when fading is started
    /// </summary>
    public event EventHandler<FadeEventArgs> FadeStart;
    /// <summary>
    /// This event occurs when fading is started
    /// </summary>
    public event EventHandler<FadeEventArgs> FadeFinish;

    protected virtual void OnFadeStart(FadeEventArgs e)
    {
        if (FadeStart != null)
            FadeStart.Invoke(this, e);
    }
    protected virtual void OnFadeFinish(FadeEventArgs e)
    {
        if (FadeFinish != null)
            FadeFinish.Invoke(this, e);
    }

    protected void Awake()
    {
        if (defaultState == FadeDirection.Out)
            fadeBalance = 0;
        else
            fadeBalance = 1;

        Init();
    }
    protected void OnGUI()
    {
        GUI.depth = GUIdepth;
    
        DrawOnGUI();
    }
    protected virtual void Update()
    {
        if (currentTask == null && tasks.Count > 0)
            StartTask();

        if (currentTask != null)
        {
            ExecuteTaskAction();

            switch (currentTask.State)
            {
                case FadeState.In:
                    fadeBalance += Time.deltaTime / currentTask.Time;
                    break;
                case FadeState.Out:
                    fadeBalance -= Time.deltaTime / currentTask.Time;
                    break;
                case FadeState.Stop:
                    fadeBalance -= 0;
                    currentTask.PostDelay -= Time.deltaTime;
                    if (currentTask.PostDelay < 0)
                        FinishTask();
                    break;
                case FadeState.InEnd:
                    fadeBalance = 1 + fadeOutDelay;
                    break;
                case FadeState.OutEnd:
                    fadeBalance = -fadeInDelay;
                    break;
            }

            if (fadeBalance > 1)
            {
                fadeBalance = 1;
                currentState = FadeState.InEnd;
                FinishTask();
            }

            if (fadeBalance < 0)
            {
                fadeBalance = 0;
                currentState = FadeState.OutEnd;
                FinishTask();
            }
        }
    }

    /// <summary>
    /// You can implement your own fading initialization by ovveride this method in successors classes
    /// </summary>
    protected virtual void Init(){ instance = this;}
    /// <summary>
    /// You can implement your own fading by ovveride this method in successors classes
    /// </summary>
    protected virtual void DrawOnGUI(){ ; }
    /// <summary>
    /// Singletone instance back field
    /// </summary>
    protected static Fader instance = null;
    /// <summary>
    /// Singletone instance of Fader
    /// </summary>
    public static IFader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(Fader)) as Fader;
                if (instance == null)
                    Debug.LogError("Fader: Could not found GameObject of type " + typeof(Fader).Name);
            }
            return instance;
        }
    }

    protected Texture GetTextureFromColor(Color color)
    {
        Texture2D texture = new Texture2D(1, 1) as Texture2D;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return (Texture)texture;
    }
    protected float GetLinearT(int i, int from)
    {
        return fadeBalance / ((float)i / (float)from);
    }
    protected float GetNonLinearT(int i, int from)
    {
        return fadeBalance * from - fadeBalance * i;
    }
}

public interface IFader
{
    /// <summary>
    /// Fade-in screen 
    /// </summary>
    IFader FadeIn(float time = 1);
    /// <summary>
    /// Fade-out screen 
    /// </summary>
    IFader FadeOut(float time = 1);
    /// <summary>
    /// Pause
    /// </summary>
    IFader Pause(float time = 1);
    /// <summary>
    /// Quick fade-in and fade-out
    /// </summary>
    IFader Flash(float inTime = 0.075f, float outTime = 0.15f);
    /// <summary>
    /// Execute an action
    /// </summary>
    IFader StartAction(IAction action);
    /// <summary>
    /// Execute an action with parameters
    /// </summary>
    IFader StartAction(IParametrizedAction action, params object[] args);
    /// <summary>
    /// Stop all fading actions
    /// </summary>
    void StopAllFadings();
    /// <summary>
    /// Start coroutine method in parallel. Next fading tasks will be starter right after coroutine was started.
    /// </summary>
    /// <param name="component">Conoutine's object. For example 'this'</param>
    /// <param name="methodName">Conoutine's method. For example "MyCoroutine"</param>
    IFader StartCoroutine(MonoBehaviour component, string methodName);
    /// <summary>
    /// Start coroutine method in parallel. Next fading tasks will be starter right after coroutine was started.
    /// </summary>
    /// <param name="component">Conoutine's object. For example 'this'</param>
    /// <param name="methodName">Conoutine's method. For example "MyCoroutine()"</param>
    /// <param name="value">Conoutine method's parameter</param>
    IFader StartCoroutine(MonoBehaviour component, string methodName, object value);
    /// <summary>
    /// Start coroutine method in series. Next fading tasks will be starter after conoutine finish. This Conoutine's methos have to have 'yield break' operator!
    /// </summary>
    /// <param name="component">Conoutine's object. For example 'this'</param>
    /// <param name="routine">Conoutine's method. For example 'MyCoroutine()'. This Conoutine's methos have to have 'yield break' operator.</param>
    IFader StartCoroutine(MonoBehaviour component, System.Collections.IEnumerator routine);
    /// <summary>
    /// Add fade-in or fade-out task to queue
    /// </summary>
    IFader Fade(FadeDirection fadeDirection, float time = 1);
    /// <summary>
    /// Add fading task to queue
    /// </summary>
    void AddTask(FaderTask task);
    /// <summary>
    /// Current state of fading
    /// </summary>
    FadeState State { get; }
    /// <summary>
    /// This event occurs when fading is started
    /// </summary>
    event EventHandler<FadeEventArgs> FadeStart;
    /// <summary>
    /// This event occurs when fading is started
    /// </summary>
    event EventHandler<FadeEventArgs> FadeFinish;

}

namespace ScreenFaderComponents
{
    /// <summary>
    /// Fading task to complete
    /// </summary>
    public class FaderTask
    {
        public FadeState State;
        public float Time;
        public float PostDelay;
        
        public IAction action;

        public IParametrizedAction pAction;
        public object[] pActionParameters;
    }

    /// <summary>
    /// Wraps the finite coroutine method
    /// </summary>
    internal class FaderCoroutine
    {
        private bool completed;
        private Exception exception = null;
        public bool Completed
        {
            get
            {
                if (exception != null)
                    throw exception;
                return completed;
            }
        }

        public Coroutine coroutine;
        public System.Collections.IEnumerator IntCoroutine(System.Collections.IEnumerator coroutine)
        {
            while (true)
            {
                try
                {
                    if (!coroutine.MoveNext())
                    {
                        completed = true;
                        yield break;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    completed = true;
                    yield break;
                }

                yield return coroutine.Current;
            }
        }
    }

    namespace Enumerators
    {
        /// <summary>
        /// Fadingdirection
        /// </summary>
        public enum FadeDirection
        {
            In,
            Out
        }

        /// <summary>
        /// Current state of fading
        /// </summary>
        public enum FadeState
        {
            In,
            Out,
            Stop,
            InEnd,
            OutEnd
        }
    }

    namespace Events
    {
        /// <summary>
        /// Fader event aruments class
        /// </summary>
        public class FadeEventArgs : EventArgs
        {
            /// <summary>
            /// Direction of completed fading (In or Out)
            /// </summary>
            public FadeDirection Direction;
        }
    }

    namespace Actions
    {
        /// <summary>
        /// Implements any parameterless action
        /// Do not foget to set Completed = true when action will finished
        /// </summary>
        public interface IAction
        {
            void Execute();
            bool Completed { get; set; }
        }

        /// <summary>
        /// Implements action with any number of parameters
        /// Do not foget to set Completed = true when action will finished
        /// </summary>
        public interface IParametrizedAction
        {
            void Execute(params object[] args);
            bool Completed { get; set; }
        }
        
        public class CoroutineAction : IParametrizedAction
        {
            FaderCoroutine result = null;

            public void Execute(params object[] parameters)
            {
                if (parameters == null || parameters.Length < 2)
                    throw new System.ArgumentOutOfRangeException();

                MonoBehaviour component = parameters[0] as MonoBehaviour;
                string methodName = parameters[1] as string;

                if (component == null || methodName == null)
                    throw new System.ArgumentNullException();

                if (result == null)
                {
                    result = new FaderCoroutine();
                    result.coroutine = component.StartCoroutine(methodName);
                }

                Completed = true;
            }
            public bool Completed { get; set; }
        }
        public class ParametrizedCoroutineAction : IParametrizedAction
        {
            FaderCoroutine result = null;

            public void Execute(params object[] parameters)
            {
                if (parameters == null || parameters.Length < 3)
                    throw new System.ArgumentOutOfRangeException();

                MonoBehaviour component = parameters[0] as MonoBehaviour;
                string methodName = parameters[1] as string;
                object value = parameters[2] as object;

                if (component == null || methodName == null)
                    throw new System.ArgumentNullException();

                if (result == null)
                {
                    result = new FaderCoroutine();
                    result.coroutine = component.StartCoroutine(methodName, value);
                }

                Completed = true;
            }
            public bool Completed { get; set; }
        }
        public class EnumeratorCoroutineAction : IParametrizedAction
        {
            FaderCoroutine result = null;
            public void Execute(params object[] parameters)
            {
                if (parameters == null || parameters.Length < 2)
                    throw new System.ArgumentOutOfRangeException();

                MonoBehaviour component = parameters[0] as MonoBehaviour;
                System.Collections.IEnumerator routine = parameters[1] as System.Collections.IEnumerator;

                if (component == null || routine == null)
                    throw new System.ArgumentNullException();

                if (result == null)
                {
                    result = new FaderCoroutine();
                    result.coroutine = component.StartCoroutine(result.IntCoroutine(routine));
                }

                Completed = result.Completed;
            }
            public bool Completed { get; set; }
        }

        /// <summary>
        /// Example of IParametrizedAction action.
        /// Load unity scene by name or index.
        /// Set as first parameter name of scene (string) or index (int) to load it.
        /// </summary>
        public class LoadSceneAction : IParametrizedAction
        {
            public void Execute(params object[] args)
            {
                if (args == null || args.Length == 0) throw new ArgumentNullException();

                string value = args[0].ToString();
                int index = 0;

                if (Int32.TryParse(value, out index))
                    UnityEngine.SceneManagement.SceneManager.LoadScene(index);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(value);

                Completed = true;
            }

            public bool Completed
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Example of IAction action.
        /// Shitch itself property 'IsLogoVisible' any time when executed.
        /// </summary>
        public class ShowLogoAction : IAction
        {
            public void Execute()
            {
                if (Completed)
                    return;

                IsLogoVisible = !IsLogoVisible;

                /// Set Completed = true to stop action execution
                Completed = true;
            }

            public bool Completed
            {
                get;
                set;
            }
            public bool IsLogoVisible
            {
                get;
                protected set;
            }
        }
    }
}

[Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
public sealed class ScreenFaderBase : MonoBehaviour
{
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public static IFader Instance
    { get { return Fader.Instance; } }
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public static void Fade(FadeDirection state)
    {
        Fader.Instance.Fade(state);
    }
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public FadeState State { get { return Fader.Instance.State; } }

    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public float fadeSpeed = 1;
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public float fadeInDelay;
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public float fadeOutDelay;
    [Obsolete("This class is obsolete, please use 'Fader' class instead of 'ScreenFaderBase'")]
    public int GUIdepth = -2;
}