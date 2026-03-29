namespace Luzart
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Notebook UI — sổ mở 2 trang, không tab.
    /// Mỗi spread hiển thị 2 items (clue hoặc character).
    /// Trang trái: text description (gạch đầu dòng tên + mô tả từng item).
    /// Trang phải: 2 ảnh polaroid (Clue1_Photo, Clue2_Photo) + nơi tìm thấy (txtClue1, txtClue2).
    /// Cả clue và character nằm chung 1 list, lật trang qua.
    /// </summary>
    public class UINotebook : UIBase
    {
        [Header("Trang trái — Description")]
        [SerializeField] private TMP_Text txtDescription;

        [Header("Trang phải — Item 1")]
        [SerializeField] private Image imgClue1Photo;
        [SerializeField] private TMP_Text txtClue1;

        [Header("Trang phải — Item 2")]
        [SerializeField] private Image imgClue2Photo;
        [SerializeField] private TMP_Text txtClue2;

        [Header("Navigation")]
        [SerializeField] private Button btnPrevPage;
        [SerializeField] private Button btnNextPage;
        [SerializeField] private TMP_Text txtPageNumber;
        [SerializeField] private Button btnClose;

        // Data — unified list (clue + character xen kẽ hoặc nối tiếp)
        private List<NotebookEntry> entries = new List<NotebookEntry>();
        private int currentPage; // mỗi page hiển thị 2 entries

        private struct NotebookEntry
        {
            public string name;
            public string description;
            public string foundLocation;
            public Sprite photo;
        }

        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnPrevPage, PrevPage);
            GameUtil.ButtonOnClick(btnNextPage, NextPage);

            if (btnClose != null)
                GameUtil.ButtonOnClick(btnClose, () => Hide());
        }

        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            BuildEntries();
            currentPage = 0;
            ShowCurrentPage();
        }

        public override void RefreshUI()
        {
            BuildEntries();
            ShowCurrentPage();
        }

        // ========== BUILD UNIFIED LIST ==========

        private void BuildEntries()
        {
            entries.Clear();

            // Clues trước
            var clues = NotebookManager.Instance.GetCollectedClues();
            for (int i = 0; i < clues.Count; i++)
            {
                var c = clues[i];
                entries.Add(new NotebookEntry
                {
                    name = c.clueName,
                    description = c.description,
                    foundLocation = c.foundLocation,
                    photo = c.clueImage
                });
            }

            // Characters sau
            var characters = NotebookManager.Instance.GetMetCharacters();
            for (int i = 0; i < characters.Count; i++)
            {
                var ch = characters[i];
                entries.Add(new NotebookEntry
                {
                    name = ch.characterName,
                    description = GetCharacterDialogueSummary(ch),
                    foundLocation = ch.foundLocation,
                    photo = ch.portrait
                });
            }
        }

        private string GetCharacterDialogueSummary(DialogueCharacterSO ch)
        {
            // Tìm dialogue tree của character này để lấy các dòng đã nói
            var collectedClues = NotebookManager.Instance.GetCollectedClues();

            // Fallback: chỉ hiện tên
            return ch.characterName;
        }

        // ========== DISPLAY ==========

        private void ShowCurrentPage()
        {
            int startIdx = currentPage * 2;
            int totalPages = Mathf.CeilToInt(entries.Count / 2f);

            // Item 1
            if (startIdx < entries.Count)
            {
                var e1 = entries[startIdx];
                ShowItem1(e1);
            }
            else
            {
                ClearItem1();
            }

            // Item 2
            if (startIdx + 1 < entries.Count)
            {
                var e2 = entries[startIdx + 1];
                ShowItem2(e2);
            }
            else
            {
                ClearItem2();
            }

            // Trang trái — description gạch đầu dòng
            BuildDescription(startIdx);

            // Nav buttons
            if (btnPrevPage != null)
                btnPrevPage.gameObject.SetActive(currentPage > 0);
            if (btnNextPage != null)
                btnNextPage.gameObject.SetActive(currentPage < totalPages - 1);
            if (txtPageNumber != null)
                txtPageNumber.text = totalPages > 0 ? $"{currentPage + 1}/{totalPages}" : "0/0";
        }

        private void BuildDescription(int startIdx)
        {
            if (txtDescription == null) return;

            if (entries.Count == 0)
            {
                txtDescription.text = "No evidence collected yet.";
                return;
            }

            var sb = new System.Text.StringBuilder();

            if (startIdx < entries.Count)
            {
                var e1 = entries[startIdx];
                sb.AppendLine($"• <b>{e1.name}</b>");
                if (!string.IsNullOrEmpty(e1.description))
                    sb.AppendLine($"  {e1.description}");
                sb.AppendLine();
            }

            if (startIdx + 1 < entries.Count)
            {
                var e2 = entries[startIdx + 1];
                sb.AppendLine($"• <b>{e2.name}</b>");
                if (!string.IsNullOrEmpty(e2.description))
                    sb.AppendLine($"  {e2.description}");
            }

            txtDescription.text = sb.ToString().TrimEnd();
        }

        // ========== ITEM DISPLAY ==========

        private void ShowItem1(NotebookEntry entry)
        {
            if (imgClue1Photo != null)
            {
                imgClue1Photo.gameObject.SetActive(true);
                if (entry.photo != null)
                    imgClue1Photo.sprite = entry.photo;
            }
            if (txtClue1 != null)
                txtClue1.text = !string.IsNullOrEmpty(entry.foundLocation) ? entry.foundLocation : "";
        }

        private void ShowItem2(NotebookEntry entry)
        {
            if (imgClue2Photo != null)
            {
                imgClue2Photo.gameObject.SetActive(true);
                if (entry.photo != null)
                    imgClue2Photo.sprite = entry.photo;
            }
            if (txtClue2 != null)
                txtClue2.text = !string.IsNullOrEmpty(entry.foundLocation) ? entry.foundLocation : "";
        }

        private void ClearItem1()
        {
            if (imgClue1Photo != null) imgClue1Photo.gameObject.SetActive(false);
            if (txtClue1 != null) txtClue1.text = "";
        }

        private void ClearItem2()
        {
            if (imgClue2Photo != null) imgClue2Photo.gameObject.SetActive(false);
            if (txtClue2 != null) txtClue2.text = "";
        }

        // ========== NAVIGATION ==========

        private void PrevPage()
        {
            if (currentPage <= 0) return;
            currentPage--;
            ShowCurrentPage();
        }

        private void NextPage()
        {
            int totalPages = Mathf.CeilToInt(entries.Count / 2f);
            if (currentPage >= totalPages - 1) return;
            currentPage++;
            ShowCurrentPage();
        }
    }
}
