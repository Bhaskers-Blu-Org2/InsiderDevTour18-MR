// MIT License
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#define EXTEND_TOOLKIT  
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.Unity.InputModule
{
    public class SpeechInputHandler : MonoBehaviour, ISpeechHandler
    {
        [Serializable]
        public struct KeywordAndResponse
        {
            [Tooltip("The keyword to handle.")]
            public string Keyword;

            [Tooltip("The handler to be invoked.")]
            public UnityEvent Response;
        }

        /// <summary>
        /// The keywords to be recognized and optional keyboard shortcuts.
        /// </summary>
        [Tooltip("The keywords to be recognized and optional keyboard shortcuts.")]
        public KeywordAndResponse[] Keywords;

        /// <summary>
        /// Determines if this handler is a global listener, not connected to a specific GameObject.
        /// </summary>
        [Tooltip("Determines if this handler is a global listener, not connected to a specific GameObject.")]
        public bool IsGlobalListener;

        /// <summary>
        /// Keywords are persistent across all scenes.  This Speech Input Source instance will not be destroyed when loading a new scene.
        /// </summary>
        [Tooltip("Keywords are persistent across all scenes.  This Speech Input Handler instance will not be destroyed when loading a new scene.")]
        public bool PersistentKeywords;

        [NonSerialized]
        private readonly Dictionary<string, UnityEvent> responses = new Dictionary<string, UnityEvent>();

        protected virtual void OnEnable()
        {
            if (IsGlobalListener)
            {
                InputManager.Instance.AddGlobalListener(gameObject);
            }
        }

        protected virtual void Start()
        {
            if (PersistentKeywords)
            {
                gameObject.DontDestroyOnLoad();
            }

            // Convert the struct array into a dictionary, with the keywords and the methods as the values.
            // This helps easily link the keyword recognized to the UnityEvent to be invoked.
            int keywordCount = Keywords.Length;
            for (int index = 0; index < keywordCount; index++)
            {
                KeywordAndResponse keywordAndResponse = Keywords[index];
                string keyword = keywordAndResponse.Keyword.ToLower();

                if (responses.ContainsKey(keyword))
                {
                    Debug.LogError("Duplicate keyword '" + keyword + "' specified in '" + gameObject.name + "'.");
                }
                else
                {
                    responses.Add(keyword, keywordAndResponse.Response);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (InputManager.Instance != null && IsGlobalListener)
            {
                InputManager.Instance.RemoveGlobalListener(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (InputManager.Instance != null && IsGlobalListener)
            {
                InputManager.Instance.RemoveGlobalListener(gameObject);
            }
        }

        void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
        {
            UnityEvent keywordResponse;

            // Check to make sure the recognized keyword exists in the methods dictionary, then invoke the corresponding method.
            if (enabled && responses.TryGetValue(eventData.RecognizedText.ToLower(), out keywordResponse))
            {
                keywordResponse.Invoke();
#if EXTEND_TOOLKIT
                if ( Feedback != null ) 
                {
                    Feedback.Play(true); 
                } 
#endif 
            }
        }

#if EXTEND_TOOLKIT
        private SpeechInputFeedback _feedback; 
        private SpeechInputFeedback Feedback
        { 
            get 
            { 
                if (_feedback == null) 
                { 
                    _feedback = GetComponent<SpeechInputFeedback>(); 
                }    
                return _feedback ; 
            } 
        }         
#endif
    }
}
