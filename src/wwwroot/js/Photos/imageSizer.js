window.imageSizer = (function(containerSel, sidebar) {
    const PERSIST_ID = "imgPerRow",
        PERSIST_VAL = localStorage.getItem(PERSIST_ID);

    let _$container = document.querySelector(containerSel),
        _$plusBtn = document.getElementById("zoom-plus"),
        _$minusBtn = document.getElementById("zoom-minus"),
        _$styler = document.createElement("style"),
        _imgPerRow = PERSIST_VAL ? parseInt(PERSIST_VAL) : 4,
        _size;

    document.body.appendChild(_$styler);
    sidebar.subscribe(update);
    document.addEventListener("resize", update);

    _$plusBtn.addEventListener("click", function() {
        _imgPerRow--;
        update();
    });

    _$minusBtn.addEventListener("click", function() {
        _imgPerRow++;
        update();
    });

    update();

    function update() {
        let newSize = _$container.clientWidth / _imgPerRow;

        _$plusBtn.disabled = _imgPerRow === 1;
        _$minusBtn.disabled = _imgPerRow === 10;

        if (newSize != _size) {
            _size = newSize;
            render();
            persist();
        }
    }

    function render() {
        let widthPct = 100 / _imgPerRow;

        _$styler.innerHTML =
            ".photo-thumbnail {" +
                "width: " + widthPct + "%;"
            "}";
    }

    function persist() {
        localStorage.setItem(PERSIST_ID, _imgPerRow);
    }

    return {
        getHeight: function() {
            return _size;
        }
    };
})("#thumbnails", window.sidebar);