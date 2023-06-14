window.multiSelect = (function() {
    let _$dependentBtns = document.getElementsByClassName("requires-selection"),
        _$selectAllBtn = document.getElementById("select-all"),
        _$unselectAllBtn = document.getElementById("unselect-all"),
        _$selectedCheckboxes = [],
        _selectedFiles = [];

    document.addEventListener("change", function() {
        update();
    })

    update();

    function update() {
        _$allCheckboxes = document.querySelectorAll(".photo-thumbnail-select"),
        _$selectedCheckboxes = document.querySelectorAll(".photo-thumbnail-select:checked"),
        _selectedFiles = [];

        for (let i = 0; i < _$selectedCheckboxes.length; i++) {
            _selectedFiles.push(_$selectedCheckboxes[i].getAttribute("data-id"));
        }

        render();
    }

    function render() {
        for (let i = 0; i < _$dependentBtns.length; i++) {
            _$dependentBtns[i].disabled = !_selectedFiles.length;
        }

        if (_$allCheckboxes.length === _$selectedCheckboxes.length) {
            _$selectAllBtn.disabled = true;
            _$unselectAllBtn.disabled = false;
        } else {
            _$selectAllBtn.disabled = false;
            _$unselectAllBtn.disabled = true;
        }
    }

    function setSelected(checked) {
        for (let i = 0; i < _$allCheckboxes.length; i++) {
            _$allCheckboxes[i].checked = checked
        }

        update();
    }

    return {
        getSelectedCheckboxes: function() {
            return _$selectedCheckboxes;
        },

        getSelectedFiles: function() {
            return _selectedFiles;
        },

        selectAll: function() {
            setSelected(true);
        },

        clearAll: function() {
            setSelected(false);
        },

        update: update
    };
})();