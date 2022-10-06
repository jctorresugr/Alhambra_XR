using System.Collections.Generic;

/// <summary>
/// The different types of action handled by this application
/// </summary>
public enum CurrentAction
{
    /// <summary>
    /// Default state
    /// </summary>
    DEFAULT          = 0,

    /// <summary>
    /// Highlight a particular area.
    /// </summary>
    IN_HIGHLIGHT     = 1,

    /// <summary>
    /// Start an annotation process
    /// </summary>
    START_ANNOTATION = 2,
}

/// <summary>
/// A pair of layer--ID to identify a data chunk
/// </summary>
public struct PairLayerID
{
    /// <summary>
    /// The layer where this data chunk belongs to
    /// </summary>
    public int Layer;

    /// <summary>
    /// Its ID inside this layer
    /// </summary>
    public int ID;
}

/// <summary>
/// The Model class of this application keeping track of all data shared by the different modules of the overall application
/// </summary>
public class Model
{
    /// <summary>
    /// Listener interface to be notified on events
    /// </summary>
    public interface IModelListener
    {
        /// <summary>
        /// Method called when the current action of the application has changed
        /// </summary>
        /// <param name="model">The model of the application calling this method</param>
        /// <param name="action">The new action</param>
        void OnSetCurrentAction(Model model, CurrentAction action);

        /// <summary>
        /// Method called when the current highlighted data chunk is updated. Note that this goes in pair with an action (CurrentAction) that needs such an information
        /// </summary>
        /// <param name="model">The model of the application calling this method</param>
        /// <param name="id">
        /// The new data chunk to highlight if the CurrentAction needs such an information.
        /// For the moment, this information is only needed in CurrentAction == CurrentAction.IN_HIGHLIGHT and in CurrentAction == CurrentAction.DEFAULT
        /// Note that if Layer == -1 or ID == -1, then there is nothing to highlight
        /// </param>
        void OnSetCurrentHighlight(Model model, PairLayerID id);
    }

    /// <summary>
    /// All the registered listeners to notify on changes in the Model
    /// </summary>
    private HashSet<IModelListener> m_listeners = new HashSet<IModelListener>();

    /// <summary>
    /// The current action to perform
    /// </summary>
    private CurrentAction m_currentAction = CurrentAction.DEFAULT;

    /// <summary>
    /// The data chunk to highlight if the current action needs such an information
    /// </summary>
    private PairLayerID m_currentHighlight = new PairLayerID(){ Layer= -1, ID= -1 };

    /// <summary>
    /// Add a new listener to notify events
    /// </summary>
    /// <param name="l">The new listener to notify on events</param>
    public void AddListener(IModelListener l)
    {
        m_listeners.Add(l);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="l">The listener to unregister</param>
    public void RemoveListener(IModelListener l)
    {
        m_listeners.Remove(l);
    }

    /// <summary>
    /// The current Action of the application to perform
    /// </summary>
    public CurrentAction CurrentAction
    {
        get => m_currentAction;
        set 
        {
            m_currentAction = value;
            foreach(IModelListener l in m_listeners)
                l.OnSetCurrentAction(this, value);
        }
    }

    /// <summary>
    /// The data chunk to highlight if the CurrentAction needs such an information.
    /// For the moment, this information is only needed in CurrentAction == CurrentAction.IN_HIGHLIGHT and in CurrentAction == CurrentAction.DEFAULT
    /// </summary>
    public PairLayerID CurrentHighlight
    {
        get => m_currentHighlight;
        set
        {
            m_currentHighlight = value;
            foreach(IModelListener l in m_listeners)
                l.OnSetCurrentHighlight(this, value);
        }
    }
}