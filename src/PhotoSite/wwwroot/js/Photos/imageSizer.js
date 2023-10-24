window.imageSizer = (function(containerSel, sidebar) {
    const PERSIST_ID = "imgPerRow",
        PERSIST_VAL = localStorage.getItem(PERSIST_ID),
        HEIGHT_BREAKPOINTS = [
            100,
            200,
            300
        ];

    let _$container = document.querySelector(containerSel),
        _$plusBtn = document.getElementById("zoom-plus"),
        _$minusBtn = document.getElementById("zoom-minus"),
        _$styler = document.createElement("style"),
        _imgPerRow = PERSIST_VAL ? parseInt(PERSIST_VAL) : 4,
        _realSize,
        _size;

    if (!_$container || !_$plusBtn)
        return;

    document.body.appendChild(_$styler);

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
        _realSize = _$container.clientWidth / _imgPerRow,
        _size = getHeightBreakpoint();

        render();
        persist();
    }

    function getHeightBreakpoint() {
        let breakpointSize = null;

        for (let i = 0; i < HEIGHT_BREAKPOINTS.length; i++) {
            if (_realSize >= HEIGHT_BREAKPOINTS[i])
                breakpointSize = HEIGHT_BREAKPOINTS[i];
        }

        if (!breakpointSize)
            if (_realSize < HEIGHT_BREAKPOINTS[0])
                breakpointSize = HEIGHT_BREAKPOINTS[0]
            else
                breakpointSize = HEIGHT_BREAKPOINTS[HEIGHT_BREAKPOINTS.length - 1];

        return breakpointSize;
    }

    function render() {
        let widthPct = 100 / _imgPerRow;

        _$styler.innerHTML =
            ".photo-thumbnail {" +
                "width: " + widthPct + "%;"
            "}";

        _$plusBtn.disabled = _imgPerRow === 1;
        _$minusBtn.disabled = _imgPerRow === 10;
    }

    function persist() {
        localStorage.setItem(PERSIST_ID, _imgPerRow);
    }

    return {
        getHeight: function() {
            return _size;
        },
        getHeightInPx: function() {
            return _realSize;
        },
        getImagesPerRow: function() {
            return _imgPerRow;
        }
    };
})("#thumbnails", window.sidebar);