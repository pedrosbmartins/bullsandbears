using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockTable : MonoBehaviour {

    public delegate void RowSelectedHandler(StockTableRow row);
    public event RowSelectedHandler OnRowSelected = delegate {};

    public delegate void RowSelectionClearedHandler();
    public event RowSelectionClearedHandler OnRowSelectionCleared = delegate {};

    public StockTableRow StockTableRowPrefab;

    private List<StockTableRow> rows = new List<StockTableRow>();
    private int? selectedRowIndex = null;

    public void InsertRow(Stock stock, Player player) {
        StockTableRow row = Instantiate(StockTableRowPrefab, transform, false);
        rows.Add(row);
        row.Setup(stock, player);
    }

    public void SelectNextRow() {
        SetCurrentRowByOffset(1);
        SelectCurrentRow();
    }

    public void SelectPreviousRow() {
        SetCurrentRowByOffset(-1);
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
        if (selectedRowIndex != null) {
            return rows[(int)selectedRowIndex];
        }
        else {
            return null;
        }
    }

    private void SetCurrentRow(int? index) {
        selectedRowIndex = index;
    }

    private void SetCurrentRowByOffset(int offset) {
        int currentIndex = selectedRowIndex ?? -1;
        int offsetIndex = currentIndex + offset;
        if (offset > 0) {
            int lastIndex = rows.Count - 1;
            selectedRowIndex = (offsetIndex > lastIndex) ? lastIndex : offsetIndex;
        }
        else if (offset < 0) {
            selectedRowIndex = (currentIndex == -1) ? rows.Count - 1
                             : (offsetIndex < 0) ? 0
                             : offsetIndex;
        }
    }

    private void SelectCurrentRow() {
        rows.ForEach(row => row.Deselect());
        CurrentRow().Select();
        HandleRowSelectionChanged();
    }

    private void HandleRowSelectionChanged() {
        if (selectedRowIndex == null) {
            OnRowSelectionCleared();
        }
        else {
            OnRowSelected(CurrentRow());
        }
    }

    private StockTableRow CurrentRow() {
        return (selectedRowIndex == null) ? null : rows[(int)selectedRowIndex];
    }

}
