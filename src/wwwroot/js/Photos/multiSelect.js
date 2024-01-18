window.multiSelect = (function() {
    let _$thumbContainer = document.body,
        _$mobileSelectOn = document.getElementById("enable-selection"),
        _$mobileSelectOff = document.getElementById("disable-selection"),
        _$dependentBtns = document.getElementsByClassName("requires-selection"),
        _$allCheckboxes = document.querySelectorAll(".photo-thumbnail-select"),
        _$selectAllBtn = document.getElementById("select-all"),
        _$unselectAllBtn = document.getElementById("unselect-all"),
        _checkboxOrder = {},
        _$selectedCheckboxes = [],
        _selectedFiles = [],
        _lastCheckboxClicked = null,
        _shiftKeyDown = false;

    _$allCheckboxes.forEach(function ($checkbox, index) {
        _checkboxOrder[$checkbox.id] = index;
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Shift")
            _shiftKeyDown = true;
    });

    document.addEventListener("keyup", function (e) {
        if (e.key === "Shift")
            setTimeout(function () {
                _shiftKeyDown = false;
            }, 500);
    });

    document.addEventListener("change", function (e) {
        if (_shiftKeyDown && _lastCheckboxClicked) {
            let startAt = _checkboxOrder[_lastCheckboxClicked.id],
                endAt = _checkboxOrder[e.target.id];

            if (startAt > endAt) {
                startAt = _checkboxOrder[e.target.id];
                endAt = _checkboxOrder[_lastCheckboxClicked.id];
            }

            for (let i = startAt; i <= endAt; i++) {
                _$allCheckboxes[i].checked = _lastCheckboxClicked.checked;
            }
        } else {
            _lastCheckboxClicked = e.target;
        }

        update();
    });

    _$mobileSelectOn.addEventListener("click", function () {
        _$thumbContainer.classList.add("has-selection");
    });

    _$mobileSelectOff.addEventListener("click", function () {
        setSelected(false);
        _$thumbContainer.classList.remove("has-selection");
    });

    update();

    function update() {
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

        if (_$selectedCheckboxes.length)
            _$thumbContainer.classList.add("has-selection");
        else
            _$thumbContainer.classList.remove("has-selection");
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