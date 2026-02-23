using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Communication
{
    /// <summary>
    /// Command pattern implementation for UI actions
    /// Useful for undo/redo, logging, and testing
    /// </summary>
    public interface IUICommand
    {
        void Execute();
        void Undo();
    }
    
    /// <summary>
    /// Command invoker/manager
    /// </summary>
    public class UICommandInvoker
    {
        private readonly Stack<IUICommand> executedCommands = new Stack<IUICommand>();
        private readonly int maxHistorySize;
        
        public UICommandInvoker(int maxHistorySize = 50)
        {
            this.maxHistorySize = maxHistorySize;
        }
        
        public void Execute(IUICommand command)
        {
            try
            {
                command.Execute();
                executedCommands.Push(command);
                
                // Limit history size
                if (executedCommands.Count > maxHistorySize)
                {
                    var overflow = executedCommands.Count - maxHistorySize;
                    var temp = new Stack<IUICommand>();
                    
                    for (int i = 0; i < maxHistorySize; i++)
                    {
                        temp.Push(executedCommands.Pop());
                    }
                    
                    executedCommands.Clear();
                    
                    while (temp.Count > 0)
                    {
                        executedCommands.Push(temp.Pop());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UICommandInvoker] Error executing command: {ex.Message}");
            }
        }
        
        public void Undo()
        {
            if (executedCommands.Count > 0)
            {
                var command = executedCommands.Pop();
                command.Undo();
            }
        }
        
        public void Clear()
        {
            executedCommands.Clear();
        }
    }
}
