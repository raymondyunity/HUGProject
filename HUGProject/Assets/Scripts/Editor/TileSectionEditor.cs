using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public class TileSectionEditor : EditorWindow
    {
        private const int NUM_ROWS = 8;
        private const int NUM_COLS = 8;

        private TileType selectedTileType = TileType.Empty;
        private TileType[,] tileSection = null;
        private string filename = "";


        [MenuItem("Window/Tile Section Editor")]
        static void Init()
        {
            TileSectionEditor window = (TileSectionEditor) EditorWindow.GetWindow(typeof (TileSectionEditor), false, "Tile Section Editor");
            window.Show();
        }

        private void OnGUI()
        {
            // TODO: Do not do it here. Initialization of tile section should happen when we
            //       provide or file or create a new tile section from scratch.
            if (tileSection == null)
            {
                InitializeTileSection();
            }

            OnGUITileSection();
            OnGUIFileInfo();
            OnGUIActions();
        }

        private void OnGUITileSection()
        {
            for (int i = 0; i < NUM_ROWS; i++)
            {
                MakeCellsRow(i);
            }

            selectedTileType = (TileType) EditorGUILayout.EnumPopup("Current selected tile type:", selectedTileType);
        }

        private void OnGUIFileInfo()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Filename:");
            filename = EditorGUILayout.TextField(filename);

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUIActions()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.skin.button.normal.textColor = Color.black;

            if (GUILayout.Button("Export Tile Section"))
            {
                if (!System.String.IsNullOrEmpty(filename))
                {
                    FileOperation.WriteResourceFile(filename, GetTileSectionAsChars());
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private char[,] GetTileSectionAsChars()
        {
            // TODO: For now we have to convert it to a char[,] until TileType enum is used in the tile generation.
            char[,] tileSectionContent = new char[NUM_ROWS, NUM_COLS];

            for (int x = 0; x < NUM_ROWS; x++)
            {
                for (int y = 0; y < NUM_COLS; y++)
                {
                    tileSectionContent[x, y] = (char) tileSection[x,y];
                }
            }

            return tileSectionContent;
        }

        private void InitializeTileSection()
        {
            tileSection = new TileType[NUM_ROWS, NUM_COLS];
            for (int x = 0; x < NUM_ROWS; x++)
            {
                for (int y = 0; y < NUM_COLS; y++)
                {
                    // TODO: Get the value from the file provided instead of that hardcoded value
                    tileSection[x, y] = TileType.Empty;
                }
            }
        }

        private void MakeCellsRow(int rowIndex)
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < NUM_COLS; i++)
            {
                string buttonLabel = ((char) tileSection[rowIndex, i]).ToString();
                GUIStyle style = GUI.skin.button;
                style.normal.textColor = tileSection[rowIndex, i] == TileType.Empty ? Color.white : Color.black;

                if (GUILayout.Button(buttonLabel, style, GUILayout.MaxHeight(50f), GUILayout.MaxWidth(50f)))
                {
                    tileSection[rowIndex, i] = selectedTileType;
                }
            }

            EditorGUILayout.EndHorizontal();
        }


        private enum TileType
        {
            Empty = 'O',
            Wall = 'X'
        };
    }
}
