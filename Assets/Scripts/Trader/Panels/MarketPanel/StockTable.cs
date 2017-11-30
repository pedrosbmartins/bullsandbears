using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockTable : MonoBehaviour {

    [SerializeField] private StockTableRow stockTableRowPrefab;

    public event Action<StockTableRow> OnRowSelected = delegate { };
    public event Action OnRowSelectionCleared = delegate { };

    private List<StockTableRow> rows = new List<StockTableRow>();
    private int? selectedRowIndex = null;

    public void InsertRow(Stock stock, Player player) {
        StockTableRow row = Instantiate(stockTableRowPrefab, transform, false);
        rows.Add(row);
        row.Setup(stock);
    }

    public void SelectNextRow() {
        SetCurrentRowIndexByOffset(1);
        SelectCurrentRow();
    }

    public void SelectPreviousRow() {
        SetCurrentRowIndexByOffset(-1);
        SelectCurrentRow();
    }

    public void DeselectRows() {
        if (selectedRowIndex != null) {
            rows.ForEach(row => row.Deselect());
            selectedRowIndex = null;
            HandleRowSelectionChanged();
        }
    }

    public StockTableRow GetCurrentRow() {
        return selectedRowIndex != null ? rows[(int)selectedRowIndex] : null;
    }

    private void SetCurrentRowIndex(int? index) {
        selectedRowIndex = index;
    }

    private void SetCurrentRowIndexByOffset(int offset) {
        int currentIndex = selectedRowIndex ?? -1;
        int offsetIndex = currentIndex + offset;
        if (offset > 0) {
            int maxIndex = rows.Count - 1;
            selectedRowIndex = (offsetIndex > maxIndex) ? maxIndex : offsetIndex;
        }
        else if (offset < 0) {
            selectedRowIndex = (currentIndex == -1) ? rows.Count - 1
                             : (offsetIndex < 0) ? 0
                             : offsetIndex;
        }
    }

    private void SelectCurrentRow() {
        rows.ForEach(row => row.Deselect());
        GetCurrentRow().Select();
        HandleRowSelectionChanged();
    }

    private void HandleRowSelectionChanged() {
        if (selectedRowIndex == null) {
            OnRowSelectionCleared();
        }
        else {
            OnRowSelected(GetCurrentRow());
        }
    }

}
