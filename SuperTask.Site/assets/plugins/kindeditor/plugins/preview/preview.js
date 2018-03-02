/*******************************************************************************
* KindEditor - WYSIWYG HTML Editor for Internet
* Copyright (C) 2006-2011 kindsoft.net
*
* @author Roddy <luolonghao@gmail.com>
* @site http://www.kindsoft.net/
* @licence http://www.kindsoft.net/license.php
*******************************************************************************/

KindEditor.plugin('preview', function (K) {
    var self = this, name = 'preview', undefined;
    self.clickToolbar(name, function () {
        var lang = self.lang(name + '.'),
            html = '<div style="padding:10px 20px;">' +
                '<iframe class="ke-textarea" frameborder="0" style="width:708px;height:400px;"></iframe>' +
                '</div>',
            dialog = self.createDialog({
                name: name,
                width: 750,
                title: self.lang(name),
                body: html
            }),
            iframe = K('iframe', dialog.div),
            doc = K.iframeDoc(iframe);
        var _html = self.fullHtml();
        alert(_html);
        _html.replace(/<embed[^>]*name="?ke-insertVideo"?[^>]*>/ig, function (tag) {
            var flashvars = K(tag).attr('flashvars');
            var url = K(tag).attr('url');
            url = 'f=' + K.getpath(url);
            _html = _html.replace(flashvars, url);
            alert(_html);
            return _html;
        });
        doc.open();
        doc.write(_html);
        doc.close();
        K(doc.body).css('background-color', '#FFF');
        iframe[0].contentWindow.focus();
    });
});
