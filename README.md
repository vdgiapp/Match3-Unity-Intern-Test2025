## **TIME**
- [7:51 PM - 22/10/2025] - Project Initialized.
- [9:36 PM - 22/10/2025] - Completed tasks (1, 2, 3).
- [9:50 PM - 22/10/2025] - Completed note down the parts that i have worked on.

## **WHATS CHANGED**
- Changed sprite of **itemNormal01 - itemNormal07** in **"Assets/Resources/prefabs"**.
---
- Added script **UIPanelGameWon.cs**.
- Added 2 SerializeField of **btnAutoplay** and **btnAutoLose** in script **UIPanelGame.cs**.
- Rewrited script **Board.cs**.
- Removed neighbour cell in **Cell.cs**.
- Removed script **BonusItem.cs**.
- Added new script **CellQueue.cs** for "Bottom Cells".
- Added **OriginCell** field in **Item.cs** to store original position.
- Rewrited script **BoardController.cs**, **GameManager.cs**
- Changed **LevelMoves.cs** to **LevelFullQueue.cs** and rewrited it.

---
### Game scene changes
- Changed name of **PanelWin** to **PanelGameWon**
- Attached script **UIPanelGameWon.cs** attached to **UIPanelGameWon**.
- Added **btnAutoplay** and **btnAutoLose** in **UIPanelGame**.
