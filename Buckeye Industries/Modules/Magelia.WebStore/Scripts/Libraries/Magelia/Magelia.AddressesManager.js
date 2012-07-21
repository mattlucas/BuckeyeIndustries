Magelia.AddressesManager = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.AddressesManager.prototype = {
    config: null,
    container: null,
    newAddressTrigger: null,
    saveAddressTrigger: null,
    cancelEditionTrigger: null,
    deleteAddressTrigger: null,
    selectAddressTrigger: null,
    addresseField: null,
    editing: null,
    addressContainer: null,
    currentAddressName: null,

    initialize: function () {
        this.addresseField = $(this.config.addressFieldSelector, this.container);
        this.newAddressTrigger = $(this.config.newAddressTriggerSelector, this.container);
        this.saveAddressTrigger = $(this.config.saveAddressTriggerSelector, this.container);
        this.cancelEditionTrigger = $(this.config.cancelEditionTriggerSelector, this.container);
        this.selectAddressTrigger = $(this.config.selectAddressTriggerSelector, this.container);
        this.deleteAddressTrigger = $(this.config.deleteAddressTriggerSelector, this.container);
        this.editing = false;
        this.setEventHandlers();
        this.refreshElementsVisibility();
        this.loadAddresses(this.config.selectedAddressId || true);
    },

    setEventHandlers: function () {
        this.addresseField.change($.proxy(this.addressChanged, this));
        this.newAddressTrigger.click($.proxy(this.newAddress, this));
        this.cancelEditionTrigger.click($.proxy(this.cancelEdition, this));
        this.saveAddressTrigger.click($.proxy(this.saveAddress, this));
        this.deleteAddressTrigger.click($.proxy(this.deleteAddress, this));
        this.selectAddressTrigger.click($.proxy(this.selectAddress, this));
    },

    selectAddress: function () {
        this.saveAddress(this.triggerSelection);
    },

    triggerSelection: function (saveResult) {
        this.container.trigger('selection', [this.addresseField.val(), saveResult.shippingAddressIsDifferent]);
    },

    deleteAddress: function () {
        $.ajax(
            {
                type: 'DELETE',
                url: this.config.deleteAddressUrl,
                data: { addressId: this.addresseField.val() },
                success: $.proxy(
                    function () {
                        this.currentAddressName = null;
                        this.editing = false;
                        this.refreshElementsVisibility();
                        this.removeAddressContainer();
                        this.loadAddresses(true);
                    },
                    this
                )
            }
        );
    },

    saveAddress: function (callBack) {
        $.ajax(
            {
                type: 'post',
                url: this.config.saveAddressUrl,
                data: Magelia.Helpers.getData(this.addressContainer),
                success: $.proxy(
                    function (response) {
                        if ($.isPlainObject(response)) {
                            if ($.isFunction(callBack)) {
                                $.proxy(callBack, this)(response);
                            }
                            else {
                                this.currentAddressName = response.name;
                                this.loadAddresses(true);
                            }
                        }
                        else {
                            this.appendAddressContainer(response);
                        }
                    },
                    this
                )
            }
        );
    },

    cancelEdition: function () {
        this.removeAddressContainer();
        this.editing = false;
        this.refreshElementsVisibility();
        this.loadAddresses(true);
    },

    removeAddressContainer: function () {
        if (this.addressContainer) {
            this.addressContainer.remove();
            this.addressContainer = null;
        }
    },

    appendAddressContainer: function (html) {
        this.removeAddressContainer();
        this.addressContainer = $(html).appendTo(this.container);
        this.editing = true,
        this.config.shippingAddressIsDifferent = false;
        this.refreshElementsVisibility();
    },

    newAddress: function () {
        this.currentAddressName = null;
        this.setMessage(this.config.newAddressText);
        $.ajax(
            {
                type: 'get',
                url: this.config.newAddressUrl,
                success: $.proxy(this.appendAddressContainer, this)
            }
        );
    },

    setMessage: function (message) {
        this.addresseField.attr({ disabled: 'disabled' }).children().remove();
        $('<option/>').text(message).appendTo(this.addresseField);
    },

    updateCurrentAddressName: function () {
        var currentAddressId = this.addresseField.val();
        this.currentAddressName = this.addresseField.children().filter(function () { return $(this).attr('value') == currentAddressId; }).first().text();
    },

    addressChanged: function () {
        this.updateCurrentAddressName();
        $.ajax(
            {
                type: 'get',
                url: this.config.getAddressUrl,
                data: { addressId: this.addresseField.val(), promptShippingAddressIsDifferent: this.config.promptShippingAddressIsDifferent, shippingAddressIsDifferent: this.config.shippingAddressIsDifferent },
                success: $.proxy(this.appendAddressContainer, this)
            }
        );
    },

    loadAddresses: function (selection) {
        $.ajax(
            {
                type: 'get',
                url: this.config.getAddressesUrl,
                success: $.proxy(
                    function (addresses) {
                        this.addresseField.removeAttr('disabled').children().remove();
                        if ($.isArray(addresses) && addresses.length > 0) {
                            for (var i = 0; i < addresses.length; i++) {
                                var address = addresses[i];
                                if (address.addressId != this.config.exceptedAddressId) {
                                    var option = $('<option/>').text(address.name).attr({ value: address.addressId }).appendTo(this.addresseField);
                                    if (address.name == this.currentAddressName) {
                                        option.attr({ selected: 'selected' })
                                    }
                                }
                            }
                            if (selection) {
                                if (typeof selection == 'string') {
                                    this.addresseField.val(selection);
                                }
                                this.addresseField.change();
                            }
                        }
                        if (this.addresseField.children().length == 0) {
                            this.setMessage(this.config.noAddressText);
                        }
                    },
                    this
                )
            }
        );
    },

    refreshElementsVisibility: function () {
        this.setElementVisibility(this.saveAddressTrigger, (!this.config.canSelect && this.editing) || (this.config.canSelect && this.editing && !this.currentAddressName));
        this.setElementVisibility(this.cancelEditionTrigger, this.editing);
        this.setElementVisibility(this.deleteAddressTrigger, this.currentAddressName);
        this.setElementVisibility(this.newAddressTrigger, (!this.editing && !this.currentAddressName) || (this.editing && this.currentAddressName));
        this.setElementVisibility(this.selectAddressTrigger, this.config.canSelect && this.currentAddressName);
    },

    setElementVisibility: function (element, visibility) {
        if (visibility) {
            element.show();
        }
        else {
            element.hide();
        }
    }
};
