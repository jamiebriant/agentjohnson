// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionHandlerBase.cs" company="Jakob Christensen">
//   Copyright (C) 2009 Jakob Christensen
// </copyright>
// <summary>
//   Represents a ActionHandlerBase.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AgentJohnson
{
    using JetBrains.ActionManagement;
    using JetBrains.Annotations;
    using JetBrains.DocumentModel;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Psi.Services;
    using JetBrains.ReSharper.Psi.Tree;
    using JetBrains.TextControl;
    using JetBrains.Util;
    using DataConstants = JetBrains.IDE.DataConstants;

    /// <summary>
    /// Represents a ActionHandlerBase.
    /// </summary>
    public abstract class ActionHandlerBase : IActionHandler
    {
        #region Implemented Interfaces

        #region IActionHandler

        /// <summary>
        /// Executes action. Called after Update, that set <c>ActionPresentation.Enabled</c> to <c>true</c>.
        /// </summary>
        /// <param name="context">
        /// The data context.
        /// </param>
        /// <param name="nextExecute">
        /// delegate to call
        /// </param>
        public void Execute([NotNull] IDataContext context, [CanBeNull] DelegateExecute nextExecute)
        {
            ISolution solution = context.GetData(DataConstants.SOLUTION);
            if (solution == null)
            {
                return;
            }

            Execute(solution, context);
        }

        /// <summary>
        /// Updates action visual presentation. If presentation.Enabled is set to <c>false</c>, Execute
        /// will not be called.
        /// </summary>
        /// <param name="context">The data context.</param>
        /// <param name="presentation">presentation to update</param>
        /// <param name="nextUpdate">delegate to call</param>
        /// <returns>The i action handler. update.</returns>
        public bool Update([NotNull] IDataContext context, [CanBeNull] ActionPresentation presentation, [CanBeNull] DelegateUpdate nextUpdate)
        {
            return Update(context);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Returns a Modification Cookie.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="document">The document.</param>
        /// <returns>A cookie.</returns>
        [NotNull]
        protected ModificationCookie EnsureWritable([CanBeNull] ISolution solution, [NotNull] IDocument document)
        {
            if (solution != null)
            {
                return DocumentManager.GetInstance(solution).EnsureWritable(document);
            }
            return new ModificationCookie(EnsureWritableResult.FAILURE);
        }

        /// <summary>
        /// Gets the element at caret.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The element at caret.
        /// </returns>
        [CanBeNull]
        protected static IElement GetElementAtCaret([NotNull] IDataContext context)
        {
            ISolution solution = context.GetData(DataConstants.SOLUTION);
            if (solution == null)
            {
                return null;
            }

            ITextControl textControl = context.GetData(DataConstants.TEXT_CONTROL);
            if (textControl == null)
            {
                return null;
            }

            return TextControlToPsi.GetElementFromCaretPosition<IElement>(solution, textControl);
        }

        /// <summary>
        /// Executes action. Called after Update, that set <c>ActionPresentation.Enabled</c> to <c>true</c>.
        /// </summary>
        /// <param name="solution">
        /// The solution.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected abstract void Execute(ISolution solution, IDataContext context);

        /// <summary>
        /// Updates the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The update.
        /// </returns>
        protected virtual bool Update([NotNull] IDataContext context)
        {
            return context.CheckAllNotNull(DataConstants.SOLUTION);
        }

        #endregion
    }
}