Magelia.BasketCount = function (container, config) {
    this.config = config;
    this.container = container;
    this.initialize();
};

Magelia.BasketCount.prototype = {
    config: null,
    container: null,

    initialize: function () {
        this.refresh();
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        $(Magelia).bind('basketChanged', $.proxy(this.refresh, this));
    },

    refresh: function () {
        if (this.config.getBasketCountUrl) {
            $.ajax(
                {
                    type: 'get',
                    url: this.config.getBasketCountUrl,
                    success: $.proxy(
                        function (html) {
                            this.container.html(html);
                        },
                        this
                    )
                }
            );
        }
    }
};