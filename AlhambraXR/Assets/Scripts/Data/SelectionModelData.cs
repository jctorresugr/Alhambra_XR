using System.Collections.Generic;
using UnityEngine;

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
/// Enumerate the possible handeness
/// </summary>
public enum Handedness
{
    /// <summary>
    /// right hand
    /// </summary>
    RIGHT = 0,

    /// <summary>
    /// left hand
    /// </summary>
    LEFT  = 1,
}


/// <summary>
/// The Model class of this application keeping track of all data shared by the different modules of the overall application
/// </summary>
public class SelectionModelData:MonoBehaviour
{
    /// <summary>
    /// Listener interface to be notified on events
    /// </summary>
    public interface ISelectionModelDataListener
    {
        /// <summary>
        /// Method called when the current action of the application has changed
        /// </summary>
        /// <param name="model">The model of the application calling this method</param>
        /// <param name="action">The new action</param>
        void OnSetCurrentAction(SelectionModelData model, CurrentAction action);

        /// <summary>
        /// Method called when the current highlighted data chunks are updated. Note that this goes in pair with an action (CurrentAction) that needs such an information
        /// </summary>
        /// <param name="model">The model of the application calling this method</param>
        /// <param name="mainID">
        /// The new main data chunk to highlight if the CurrentAction needs such an information.
        /// For the moment, this information is only needed in CurrentAction == CurrentAction.IN_HIGHLIGHT and in CurrentAction == CurrentAction.DEFAULT
        /// Note that if Layer == -1 or ID == -1, then there is nothing to highlight
        /// </param>
        /// <param name="secondID">
        /// The second main data chunk to highlight if the CurrentAction needs such an information.
        /// For the moment, this information is only needed in CurrentAction == CurrentAction.DEFAULT
        /// Note that if Layer == -1 or ID == -1, then there is nothing to highlight
        /// </param>
        void OnSetCurrentHighlight(SelectionModelData model, AnnotationID mainID, AnnotationID secondID);

        void OnSetSelectedAnnotations(SelectionModelData model, List<AnnotationID> ids);
    }

    /// <summary>
    /// All the registered listeners to notify on changes in the Model
    /// </summary>
    private HashSet<ISelectionModelDataListener> m_listeners = new HashSet<ISelectionModelDataListener>();

    /// <summary>
    /// The current action to perform
    /// </summary>
    private CurrentAction m_currentAction = CurrentAction.DEFAULT;

    /// <summary>
    /// The main data chunk to highlight if the current action needs such an information
    /// </summary>
    private AnnotationID m_currentHighlightMain = AnnotationID.INVALID_ID;

    /// <summary>
    /// The second data chunk to highlight if the current action needs such an information
    /// </summary>
    private AnnotationID m_currentHighlightSecond = AnnotationID.INVALID_ID;

    private List<AnnotationID> m_selectedAnnotations = new List<AnnotationID>();

    public IReadOnlyList<AnnotationID> SelectedAnnotations
    {
        get => m_selectedAnnotations;
        set
        {
            m_selectedAnnotations.Clear();
            m_selectedAnnotations.AddRange(value);
            TriggerChangeSelectedAnnotation();
        }
    }

    public void AddSelectedAnnotations(AnnotationID annot)
    {
        m_selectedAnnotations.Add(annot);
        TriggerChangeSelectedAnnotation();
    }

    public void ClearSelectedAnnotations()
    {
        m_selectedAnnotations.Clear();
        TriggerChangeSelectedAnnotation();
    }


    protected void TriggerChangeSelectedAnnotation()
    {
        foreach (ISelectionModelDataListener l in m_listeners)
            l.OnSetSelectedAnnotations(this, m_selectedAnnotations);
    }
    /// <summary>
    /// Add a new listener to notify events
    /// </summary>
    /// <param name="l">The new listener to notify on events</param>
    public void AddListener(ISelectionModelDataListener l)
    {
        m_listeners.Add(l);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="l">The listener to unregister</param>
    public void RemoveListener(ISelectionModelDataListener l)
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
            foreach(ISelectionModelDataListener l in m_listeners)
                l.OnSetCurrentAction(this, value);
        }
    }

    /// <summary>
    /// The main data chunk to highlight if the CurrentAction needs such an information.
    /// For the moment, this information is only needed in CurrentAction == CurrentAction.IN_HIGHLIGHT and in CurrentAction == CurrentAction.DEFAULT
    /// </summary>
    public AnnotationID CurrentHighlightMain
    {
        get => m_currentHighlightMain;
        set
        {
            m_currentHighlightMain = value;
            foreach(ISelectionModelDataListener l in m_listeners)
                l.OnSetCurrentHighlight(this, value, m_currentHighlightSecond);
        }
    }

    /// <summary>
    /// The second data chunk to highlight if the CurrentAction needs such an information.
    /// For the moment, this information is only needed in CurrentAction == CurrentAction.DEFAULT
    /// </summary>
    public AnnotationID CurrentHighlightSecond
    {
        get => m_currentHighlightSecond;
        set
        {
            m_currentHighlightSecond = value;
            foreach (ISelectionModelDataListener l in m_listeners)
                l.OnSetCurrentHighlight(this, m_currentHighlightMain, value);
        }
    }
}