// Prototype Collection - Street Props
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;

namespace AmplifyCreations.PrototypeCollection.StreetProps
{
    public class Preferences
    {
        private static readonly GUIContent StartUp =
            new GUIContent("Show start screen on Unity launch");

        public const string PrefHashGUIDBase64 =
            "Kz/ZJqsj7kSIf9yCXV4UdA==";

        // Do not use Application.productName here.
        // Unity 6 can throw an exception when productName
        // is accessed during ScriptableObject/editor initialization.
        public const string PrefStartUp =
            PrefHashGUIDBase64 + "StreetProps";

        public static bool GlobalStartUp = true;
    }
}