$(function () {
    $.ajaxSetup({ cache: false });
    $(document).on("click", "a[data-modal]", function (e) {
        $('#myModalContent').load(this.href, function () {
            $('#myModal').modal({
                backdrop: 'static', keyboard: false
            }, 'show');
            //bindForm(this);
        });
        return false;
    });
});

$(function () {
    $.ajaxSetup({ cache: false });
    $(document).on("click", "a[data-modal1]", function (e) {
        $('#myModal').modal('hide');
        $('#myModalContent1').load(this.href, function () {
            $('#myModal1').modal({
                backdrop: 'static', keyboard: false
            }, 'show');
            bindForm(this);
        });
        return false;
    });
});

function bindForm(dialog) {
    $('form', dialog).submit(function () {
        $('#progress').show();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (result) {
                $.simplyToast(result.Msg, result.Status);
            }
        });
        return false;
    });
}