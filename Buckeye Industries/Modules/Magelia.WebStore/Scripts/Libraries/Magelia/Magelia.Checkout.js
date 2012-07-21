Magelia.Checkout = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.Checkout.prototype = {
    config: null,
    container: null,
    shippingAddressStep: null,
    billingAddressStep: null,
    shippingRatesStep: null,
    paymentStep: null,

    initialize: function () {
        this.getSteps();
        this.setEventHandlers();
        this.editBillingAddress();
    },

    getSteps: function () {
        var steps = this.container.children();
        this.billingAddressStep = steps.eq(0);
        this.shippingAddressStep = steps.eq(1);
        this.shippingRatesStep = steps.eq(2);
        this.paymentStep = steps.eq(3);
    },

    setEventHandlers: function () {
        var self = this;
        $(this.config.modifiyTriggerSelector, this.container).hide().click(function () { self.modify($(this)); });
    },

    modify: function (trigger) {
        if ($.contains(this.billingAddressStep[0], trigger[0])) {
            this.resetFromStep(this.billingAddressStep);
            this.editBillingAddress();
        }
        else if ($.contains(this.shippingAddressStep[0], trigger[0])) {
            this.resetFromStep(this.shippingAddressStep);
            this.editShippingAddress();
        }
        else if ($.contains(this.shippingRatesStep[0], trigger[0])) {
            this.resetFromStep(this.shippingRatesStep);
            this.editShippingRates();
        }
    },

    setBillingAddressEventHandlers: function () {
        var self = this;
        $('.mag-addressesManager', this.billingAddressStep).bind('selection', $.proxy(this.selectBillingAddress, this));
        $('.mag-address', this.billingAddressStep).bind('next', function () { self.registerBillingAddress($(this)); });
    },

    setShippingAddressEventHandlers: function () {
        var self = this;
        $('.mag-addressesManager', this.shippingAddressStep).bind('selection', $.proxy(this.selectShippingAddress, this));
        $('.mag-address', this.shippingAddressStep).bind('next', function () { self.registerShippingAddress($(this)); });
    },

    setShippingRatesEventHandlers: function () {
        var self = this;
        $('.mag-shippingRates', this.shippingRatesStep).bind('next', function () { self.setShippingRates($(this)); });
    },

    setPaymentEventHandlers: function () {
        var self = this;
        $('.mag-payment', this.paymentStep).bind('order', function () { self.saveAsOrder($(this)); })
    },

    getShippingRatesRecap: function () {
        this.updateStep(
            this.shippingRatesStep,
            this.config.getShippingRatesRecapUrl,
            false,
            null,
            true
        );
    },

    saveAsOrder: function (payment) {
        this.updateStep(
            this.paymentStep,
            this.config.saveAsOrderUrl,
            true,
            Magelia.Helpers.getData(payment),
            false,
            function () {
                var proceedToPayment = $('.mag-proceedToPayment', this.paymentStep);
                if (proceedToPayment.length == 1) {
                    this.triggerBasketChanged();
                    this.proceedToPayment(proceedToPayment);
                }
                else {
                    this.setPaymentEventHandlers();
                }
            }
        );
    },

    blockModifies: function () {
        this.allowModify(this.billingAddressStep, false);
        this.allowModify(this.shippingAddressStep, false);
        this.allowModify(this.shippingRatesStep, false);
    },

    proceedToPayment: function (proceedToPayment) {
        this.blockModifies();
        if ($('input', proceedToPayment).length > 0) {
            proceedToPayment.submit();
        }
    },

    setShippingRates: function (shippingRates) {
        this.updateStep(
            this.shippingRatesStep,
            this.config.setShippingRatesUrl,
            true,
            Magelia.Helpers.getData(shippingRates),
            false,
            function (response) {
                if ($.isPlainObject(response) && response.success) {
                    this.editPayment(response.basketHash);
                    this.getShippingRatesRecap();
                    this.triggerBasketChanged();
                }
                else {
                    this.setShippingRatesEventHandlers();
                }
            }
        );
    },

    editPayment: function (basketHash) {
        this.updateStep(
            this.paymentStep,
            this.config.editPaymentUrl,
            false,
            { basketHash: basketHash },
            false,
            this.setPaymentEventHandlers
        );
    },

    editBillingAddress: function () {
        this.updateStep(
            this.billingAddressStep,
            this.config.editBillingAddressUrl,
            false,
            null,
            false,
            this.setBillingAddressEventHandlers
        );
    },

    editShippingAddress: function () {
        this.updateStep(
            this.shippingAddressStep,
            this.config.editShippingAddressUrl,
            false,
            null,
            false,
            this.setShippingAddressEventHandlers
        );
    },

    getBillingAddressRecap: function () {
        this.updateStep(
            this.billingAddressStep,
            this.config.getBillingAddressRecapUrl,
            false,
            null,
            true
        );
    },

    getShippingAddressRecap: function () {
        this.updateStep(
            this.shippingAddressStep,
            this.config.getShippingAddressRecapUrl,
            false,
            null,
            true
        );
    },

    editShippingRates: function () {
        this.updateStep(
            this.shippingRatesStep,
            this.config.editShippingRatesUrl,
            false,
            null,
            false,
            this.setShippingRatesEventHandlers
        );
    },

    registerBillingAddress: function (address) {
        this.updateStep(
            this.billingAddressStep,
            this.config.registerBillingAddressUrl,
            true,
            Magelia.Helpers.getData(address),
            false,
            function (response) {
                if ($.isPlainObject(response)) {
                    this.getBillingAddressRecap();
                    if (response.shippingAddressIsDifferent) {
                        this.editShippingAddress();
                    }
                    else {
                        this.triggerBasketAndLocationChanged();
                        this.editShippingRates();
                    }
                }
                else {
                    this.setBillingAddressEventHandlers();
                }
            }
        );
    },

    registerShippingAddress: function (address) {
        this.updateStep(
            this.shippingAddressStep,
            this.config.registerShippingAddressUrl,
            true,
            Magelia.Helpers.getData(address),
            false,
            function (response) {
                if ($.isPlainObject(response) && response.success) {
                    this.triggerBasketAndLocationChanged();
                    this.getShippingAddressRecap();
                    this.editShippingRates();
                }
                else {
                    this.setShippingAddressEventHandlers();
                }
            }
        );
    },

    selectBillingAddress: function (event, addressId, shippingAddressIsDifferent) {
        var data = { billingAddressId: addressId, shippingAddressIsDifferent: shippingAddressIsDifferent };
        var antiForgeryToken = Magelia.Helpers.getAntiforgeryToken();
        data[antiForgeryToken.attr('name')] = antiForgeryToken.val();
        this.updateStep(
            this.billingAddressStep,
            this.config.selectBillingAddressUrl,
            true,
            data,
            true,
            function () {
                if (shippingAddressIsDifferent) {
                    this.editShippingAddress();
                }
                else {
                    this.triggerBasketAndLocationChanged();
                    this.editShippingRates();
                }
            }
        );
    },

    selectShippingAddress: function (event, addressId) {
        var data = { shippingAddressId: addressId };
        var antiForgeryToken = Magelia.Helpers.getAntiforgeryToken();
        data[antiForgeryToken.attr('name')] = antiForgeryToken.val();
        this.updateStep(
            this.shippingAddressStep,
            this.config.selectShippingAddressUrl,
            true,
            data,
            true,
            function () {
                this.triggerBasketAndLocationChanged();
                this.editShippingRates();
            }
        );
    },

    triggerBasketAndLocationChanged: function () {
        this.triggerBasketChanged();
        this.triggerLocationChanged();
    },

    triggerBasketChanged: function () {
        $(Magelia).trigger('basketChanged');
    },

    triggerLocationChanged: function () {
        $(Magelia).trigger('locationChanged');
    },

    updateStep: function (step, url, post, data, allowModify, callBack) {
        $.ajax(
            {
                type: post ? 'post' : 'get',
                url: url,
                data: data,
                success: $.proxy(
                    function (response) {
                        if (!$.isPlainObject(response)) {
                            this.updateStepContent(step, response);
                        }
                        this.allowModify(step, allowModify);
                        if ($.isFunction(callBack)) {
                            $.proxy(callBack, this)(response);
                        }
                    },
                    this
                ),
                error: $.proxy(this.showError, this)
            }
        );
    },

    resetFromStep: function (step) {
        var self = this;
        step.parent().children().slice(step.index()).each(
            function () {
                var step = $(this);
                self.allowModify(step, false);
                $('.mag-content', step).remove();
            }
        );
    },

    allowModify: function (step, allowModify) {
        var modify = $(this.config.modifiyTriggerSelector, step);
        if (allowModify) {
            modify.show();
        }
        else {
            modify.hide();
        }
    },

    updateStepContent: function (step, html) {
        $('.mag-content', step).remove();
        $('<div/>').addClass('mag-content').appendTo(step).html(html);
    },

    showError: function () {
        alert(this.config.errorMessage);
    }
};