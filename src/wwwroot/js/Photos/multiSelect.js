window.multiSelect = (function() {
    let _$dependentBtns = document.getElementsByClassName("requires-selection"),
        _$selectedCheckboxes = [],
        _selectedFiles = [];

    document.addEventListener("change", function() {
        update();
    })

    update();

    function update() {
        _$selectedCheckboxes = document.querySelectorAll(".photo-thumbnail-select:checked");
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
    }

    return {
        getSelectedCheckboxes: function() {
            return _$selectedCheckboxes;
        },

        getSelectedFiles: function() {
            return _selectedFiles;
        },

        update: update
    };
})();