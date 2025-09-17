using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlayerController;
using System;

public class UISlidingPuzzleManager : MonoBehaviour
{
    public static UISlidingPuzzleManager instance;

    [Header("UI Setup")]
    [SerializeField] private RectTransform puzzleContainer;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private Texture2D puzzleImage;
    [SerializeField] private Button closeBtn;
    public UIAnimationController Container;

    [Header("Puzzle Settings")]
    [SerializeField] private int size = 3;
    [SerializeField] private float gapSize = 5f;
    [SerializeField] private bool emptyAtStart = true; // Empty ở vị trí đầu (0,0)

    [Header("Animation")]
    [SerializeField] private float moveAnimationDuration = 0.2f;

    private List<GameObject> pieces; // Danh sách các piece objects
    private int emptyIndex; // Vị trí hiện tại của ô trống
    private Vector2 pieceSize;
    private bool isMoving = false;

    public event System.Action PuzzleCompleted;
  
    private void Awake()
    {
      
            instance = this;
        
    }

    void Start()
    {
        
    }

    public void InitializePuzzle(Texture2D Image, Action quitAction = null)
    {
        pieces = new List<GameObject>();

        if (puzzleContainer == null)
            puzzleContainer = GetComponent<RectTransform>();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(delegate
        {
            Debug.Log("asd");
            quitAction?.Invoke();
        });

        CreatePuzzlePieces(Image);
        StartCoroutine(ShuffleAfterDelay(1f));
    }

    void CreatePuzzlePieces(Texture2D Image)
    {
        // Clear existing pieces
        for(int i = 0; i < puzzleContainer.childCount; i++)
        {
            Destroy(puzzleContainer.GetChild(i).gameObject);
        }    
        pieces = new List<GameObject>();

        // Calculate piece size
        Vector2 containerSize = puzzleContainer.rect.size;
        float totalGapX = gapSize * (size - 1);
        float totalGapY = gapSize * (size - 1);

        pieceSize = new Vector2(
            (containerSize.x - totalGapX) / size,
            (containerSize.y - totalGapY) / size
        );

        // Create pieces
        for (int i = 0; i < size * size; i++)
        {
            int row = i / size;
            int col = i % size;

            GameObject pieceObj = Instantiate(piecePrefab, puzzleContainer);
            pieces.Add(pieceObj);

            // Setup RectTransform
            RectTransform pieceRect = pieceObj.GetComponent<RectTransform>();
            pieceRect.sizeDelta = pieceSize;
            pieceRect.anchorMin = new Vector2(0, 1);
            pieceRect.anchorMax = new Vector2(0, 1);

            Vector2 position = new Vector2(
                col * (pieceSize.x + gapSize),
                -row * (pieceSize.y + gapSize)
            );
            pieceRect.anchoredPosition = position;

            // Setup piece
            //if (emptyAtStart && i == 0) // Empty at position 0
            //{
            //    emptyIndex = 0;
            //    //pieceObj.SetActive(false);
            //    pieceObj.name = "EmptySpace";
            //}
            //else if (!emptyAtStart && i == size * size - 1) // Empty at last position
            //{
            //    emptyIndex = size * size - 1;
            //    //pieceObj.SetActive(false);
            //    pieceObj.name = "EmptySpace";
            //}

            SetupPieceVisual(pieceObj, i, row, col, Image);
            SetupPieceButton(pieceObj, i);
        }
    }

    void SetupPieceVisual(GameObject pieceObj, int index, int row, int col, Texture2D Image)
    {
        Image pieceImage = pieceObj.GetComponent<Image>();
        if (pieceImage == null)
            pieceImage = pieceObj.AddComponent<Image>();

        if (Image != null)
        {
            // Create sprite from puzzle image
            int pieceWidth = Image.width / size;
            int pieceHeight = Image.height / size;

            Rect spriteRect = new Rect(
                col * pieceWidth,
                (size - 1 - row) * pieceHeight, // Flip Y
                pieceWidth,
                pieceHeight
            );

            Sprite pieceSprite = Sprite.Create(Image, spriteRect, new Vector2(0.5f, 0.5f));
            pieceImage.sprite = pieceSprite;
            pieceImage.color = Color.white;
        }
        else
        {
            // Fallback color and number
            float hue = (index * 0.618f) % 1f;
            pieceImage.color = Color.HSVToRGB(hue, 0.6f, 0.9f);

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(pieceObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.text = (index + 1).ToString();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(pieceSize.x * 0.3f);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        pieceObj.name = $"Piece_{index}";
    }

    void SetupPieceButton(GameObject pieceObj, int index)
    {
        Button button = pieceObj.GetComponent<Button>();
        if (button == null)
            button = pieceObj.AddComponent<Button>();

        button.onClick.AddListener(() => OnPieceClicked(index));
    }

    void OnPieceClicked(int originalIndex)
    {
        if (isMoving) return;

        // Find current position of this piece
        int currentPos = FindPiecePosition(originalIndex);
        if (currentPos == -1) return;

        // Check if can move to empty space
        if (CanMovePiece(currentPos, emptyIndex))
        {
            StartCoroutine(MovePieceToEmpty(currentPos));
        }
    }

    int FindPiecePosition(int originalIndex)
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name == $"Piece_{originalIndex}")
            {
                return i;
            }
        }
        return -1;
    }

    bool CanMovePiece(int piecePos, int emptyPos)
    {
        if(emptyPos == size * size)
        {
            return (piecePos == size - 1);
        }

        if (piecePos == size * size && emptyPos == size - 1) return true;

        int pieceRow = piecePos / size;
        int pieceCol = piecePos % size;
        int emptyRow = emptyPos / size;
        int emptyCol = emptyPos % size;

        // Check if adjacent
        bool sameRow = (pieceRow == emptyRow) && Mathf.Abs(pieceCol - emptyCol) == 1;
        bool sameCol = (pieceCol == emptyCol) && Mathf.Abs(pieceRow - emptyRow) == 1;

        return sameRow || sameCol;
    }

    IEnumerator MovePieceToEmpty(int piecePosition)
    {
        isMoving = true;

        GameObject pieceToMove = pieces[piecePosition];
        Vector2 targetPosition = GetPositionForIndex(emptyIndex);

        if (moveAnimationDuration > 0)
        {
            Vector2 startPos = pieceToMove.GetComponent<RectTransform>().anchoredPosition;
            float elapsed = 0f;

            while (elapsed < moveAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveAnimationDuration;
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth animation

                pieceToMove.GetComponent<RectTransform>().anchoredPosition =
                    Vector2.Lerp(startPos, targetPosition, t);

                yield return null;
            }
        }

        pieceToMove.GetComponent<RectTransform>().anchoredPosition = targetPosition;

        // Swap pieces in list
        
        (pieces[piecePosition], pieces[emptyIndex]) = (pieces[emptyIndex], pieces[piecePosition]);

        emptyIndex = piecePosition;

        isMoving = false;

        // Check win condition
        if (CheckWinCondition())
        {
            OnPuzzleComplete();
        }
    }

    Vector2 GetPositionForIndex(int index)
    {
        if(index == size * size)
        {
            return new Vector2(
            (size - 1) * (pieceSize.x + gapSize),
            1 * (pieceSize.y + gapSize)
        );
        }

        int row = index / size;
        int col = index % size;

        return new Vector2(
            col * (pieceSize.x + gapSize),
            -row * (pieceSize.y + gapSize)
        );
    }

    bool CheckWinCondition()
    {
        // Check if empty is in correct position
        int correctEmptyPos = size * size;
        if (emptyIndex != correctEmptyPos) return false;

        // Check if all pieces are in correct positions
        for (int i = 0; i < pieces.Count; i++)
        {
            if (i == correctEmptyPos) continue; // Skip empty space

            if (pieces[i].name != $"Piece_{i}")
                return false;
        }

        return true;
    }

    void OnPuzzleComplete()
    {
        Debug.Log("Puzzle Complete!");

        // Show empty piece temporarily
        pieces[emptyIndex].SetActive(true);

        PuzzleCompleted?.Invoke();

        // Auto shuffle after delay
       StartCoroutine(DestroyPiece());
    }

    IEnumerator DestroyPiece()
    {
        yield return new WaitForSeconds(1f);

        foreach (Transform child in puzzleContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    IEnumerator ShuffleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        emptyIndex = size - 1; //top right
        // Hide empty piece
        pieces[emptyIndex].SetActive(true);

        // Perform shuffle
        for (int i = 0; i < size * size * 10; i++)
        {
            List<int> validMoves = GetValidMoves();
            if (validMoves.Count > 0)
            {
                int randomMove = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];
                SwapInstant(randomMove, emptyIndex);
            }
        }

        while(emptyIndex != size - 1)
        {
            List<int> validMoves = GetValidMoves();
            if (validMoves.Count > 0)
            {
                int randomMove = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];
                SwapInstant(randomMove, emptyIndex);
            }
        }

        GameObject emptyPiece = Instantiate(piecePrefab, puzzleContainer);
        emptyPiece.transform.SetAsFirstSibling();
        RectTransform emptyRect = emptyPiece.GetComponent<RectTransform>();
        emptyRect.sizeDelta = pieceSize;
        emptyRect.anchorMin = new Vector2(0, 1);
        emptyRect.anchorMax = new Vector2(0, 1);
        emptyRect.anchoredPosition = GetPositionForIndex(size * size);

        pieces.Add(emptyPiece);
        emptyIndex = size * size;
        
        Container.Activate();
    }

    List<int> GetValidMoves()
    {
        List<int> validMoves = new List<int>();

        int emptyRow = emptyIndex / size;
        int emptyCol = emptyIndex % size;

        // Check four directions
        int[] directions = { -size, size, -1, 1 }; // Up, Down, Left, Right

        foreach (int dir in directions)
        {
            int newPos = emptyIndex + dir;

            if (newPos >= 0 && newPos < pieces.Count)
            {
                int newRow = newPos / size;
                int newCol = newPos % size;

                // Check for horizontal wrapping
                if (dir == -1 && emptyCol == 0) continue;
                if (dir == 1 && emptyCol == size - 1) continue;

                validMoves.Add(newPos);
            }
        }

        return validMoves;
    }

    void SwapInstant(int pos1, int pos2)
    {
        // Swap pieces
        (pieces[pos1], pieces[pos2]) = (pieces[pos2], pieces[pos1]);

        // Update positions
        pieces[pos1].GetComponent<RectTransform>().anchoredPosition = GetPositionForIndex(pos1);
        pieces[pos2].GetComponent<RectTransform>().anchoredPosition = GetPositionForIndex(pos2);

        // Update empty index
        emptyIndex = pos1;
    }

    // Public methods
    public void SetPuzzleImage(Texture2D newImage)
    {
        puzzleImage = newImage;
        CreatePuzzlePieces(puzzleImage);
    }

    public void SetSize(int newSize)
    {
        if (newSize >= 2 && newSize <= 6)
        {
            size = newSize;
            //CreatePuzzlePieces(puzzleImage);
        }
    }

    public void ToggleEmptyPosition()
    {
        emptyAtStart = !emptyAtStart;
        CreatePuzzlePieces(puzzleImage);
    }
}