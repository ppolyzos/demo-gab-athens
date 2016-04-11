// Write your Javascript code.
; (function (window, $, TweenMax) {

    // For Demo purposes only (show hover effect on mobile devices)
    [].slice.call(document.querySelectorAll('a[href="#"]')).forEach(function (el) {
        el.addEventListener('click', function (ev) { ev.preventDefault(); });
    });

    function extend(a, b) {
        for (var key in b) {
            if (b.hasOwnProperty(key)) {
                a[key] = b[key];
            }
        }
        return a;
    }

    var fakeLoading = function ($obj, speed, failAt) {
        if (typeof speed == "undefined") speed = 2;
        if (typeof failAt == "undefined") failAt = -1;
        var v = 0;
        var l = function () {
            if (failAt > -1) {
                if (v >= failAt) {
                    if (typeof $obj.jquery != "undefined") {
                        $obj.ElasticProgress("fail");
                    } else {
                        $obj.fail();
                    }
                    return;
                }
            }
            v += Math.pow(Math.random(), 2) * 0.1 * speed;

            if (typeof $obj.jquery != "undefined") {
                $obj.ElasticProgress("setValue", v);
            } else {
                $obj.setValue(v);
            }
            if (v < 1) {
                TweenMax.delayedCall(0.05 + (Math.random() * 0.14), l);
            }
        };
        l();
    };

    function gwab(el, options) {
        this.gridEl = el;
        this.options = extend({}, this.options);
        extend(this.options, options);

        this._init();
    };

    gwab.prototype.options = {
        photos: {},
        faces: {}
    };

    gwab.prototype._initFileUploadFallback = function () {
        if (!this.options.upload) return;

        this.options.upload.inputFileUpload.fileupload({
            dataType: 'json',
            start: function () {
                console.log('started uploading');
            },
            done: function (e, data) {
                console.log('done uploading');
            },
            error: function (e, data) {
                console.log('error in uploading');
            }
        });
    };

    gwab.prototype._displayFileUpload = function (progress) {
        if (!this.options.upload) return;

        this.options.upload.inputFileUpload.fileupload({
            dataType: 'json',
            start: function () {
                progress.ElasticProgress('open');
            },
            done: function (e, data) {
                progress.ElasticProgress('close');
            },
            error: function (e, data) {
                progress.ElasticProgress('fail');
                setTimeout(function () {
                    progress.ElasticProgress('close');
                }, 2000);
            },
            progress: function (e, data) {
                var v = parseInt(data.loaded / data.total * 100, 10);
                fakeLoading(progress);
            }
        }).trigger('click');
    }

    gwab.prototype._init = function () {
        this._bindProgressBar();
        this._bindPhotoEvents();
        this._bindFacesEvents();
        this._bindResetEvents();

        var canSvg = !!(document.createElementNS && document.createElementNS('http://www.w3.org/2000/svg', 'svg').createSVGRect);
        console.log('Can svg', canSvg);
        if (!canSvg) {
            this._initFileUploadFallback();
            $('.box.box--centered .upload').hide();
            $('.file-upload').show();
        }
    };

    gwab.prototype._bindProgressBar = function () {
        var self = this;
        if (!self.options.upload) return;

        var progress = self.options.upload.effectEl.ElasticProgress({
            buttonSize: 60,
            fontFamily: "Montserrat",
            colorBg: "#adeca8",
            colorFg: "#7cc576",
            arrowDirection: "up",
            onClick: function (event) {
                console.log("onClick");
                self._displayFileUpload(progress);
            }
        });
    }

    gwab.prototype._bindPhotoEvents = function () {
        if (!this.options.photos.grid) return;

        this.options.photos.grid.on('click', 'figure > img', function (e) {
            e.preventDefault();
            var $this = $(this);
            $this.parent('figure').toggleClass('faces');
        });
    };

    gwab.prototype._bindFacesEvents = function () {
        var self = this;
        if (!self.options.upload) return;

        $(self.options.photos.grid).on('click', '.face', function (e) {
            e.preventDefault();
            var $this = $(this);
            var face = self.options.faces.grid.find('a[href="' + $this.attr('href') + '"]');
            face.parent('.face').find('.attributes').slideToggle();
            console.log($this.attr('href'));
        });

        self.options.faces.grid.on('click', '.face', function (e) {
            e.preventDefault();
            console.log('show face');
            $(this).find('.attributes').slideToggle('normal');
        });

        var viewAllFaces = false;
        $('#btn-show-faces').on('click', function (e) {
            e.preventDefault();
            console.log('show faces');
            self.options.faces.grid
                .find('.face .attributes')
                [viewAllFaces ? 'slideUp' : 'slideDown']('normal');

            viewAllFaces = !viewAllFaces;
        });
    };

    gwab.prototype._bindResetEvents = function () {
        var self = this;
        if (!self.options.reset) return;

        self.options.reset.on('click', function (e) {
            e.preventDefault();
            console.log('reset');
            self.options.hub.server.reset();

            var photoEmptyMsg = self.options.photos.grid.find('.empty-msg')
                .clone().show();

            var facesEmptyMsg = self.options.faces.grid.find('.empty-msg')
                .clone().show();

            self.options.photos.grid.empty();
            self.options.faces.grid.empty();

            self.options.photos.grid.append(photoEmptyMsg);
            self.options.faces.grid.append(facesEmptyMsg);

        });
    };

    gwab.prototype.addPhoto = function (photo) {
        var self = this;
        var photoGrid = self.options.photos.grid;
        var tplPhoto = self.options.photos.tpl;
        var assetsUrl = self.options.assetsUrl;

        photoGrid.find('.empty-msg').hide();

        var template = tplPhoto.clone();
        template.attr('id', 'photo-' + photo.Id);
        template.find('img').attr('src', assetsUrl + '/photos/' + photo.Filename + '.jpeg');
        template.removeClass('hidden');
        photoGrid.prepend(template);
    };

    gwab.prototype.addFaces = function (faces) {
        var self = this;
        var photoGrid = self.options.photos.grid;
        var facesGrid = self.options.faces.grid;
        var assetsUrl = self.options.assetsUrl;


        facesGrid.find('.empty-msg').hide();

        for (var i = 0, len = faces.length; i < len; i++) {
            var face = faces[i];

            if (!face) return;
            console.log('faces', face);

            var photo = photoGrid.find('#photo-' + face.PhotoId);
            var iconLinks = photo.find('.icon-links');
            var tplFace = self.options.faces.tpl.clone().removeClass('hidden');
            var url = [assetsUrl, '/faces/', face.Id, '.jpeg'].join('');
            tplFace.find('a').attr('href', face.Id);
            tplFace.find('img').attr('src', url);
            var link = ['<a href="', face.Id, '" class="face"><img src=', url, ' /></a>'].join('');

            if (face.Age) {
                tplFace.find('.attributes span:eq(0)').text(face.Age);
            }

            tplFace.find('.attributes span:eq(1)').text(face.Gender === 0 ? 'Male' : 'Female');

            if (face.Smile !== null) {
                var smile = Math.round(face.Smile * 10000) / 100;

                tplFace.find('.attributes span.smile').text(smile + '%');
                tplFace.find('.attributes span.progress-inner').css('width', smile / 2 + 'px');
            }

            iconLinks.append(link);
            facesGrid.append(tplFace);

            photo.find('figure').toggleClass('faces', true);
        }
    };

    window.gwab = gwab;
})(window, jQuery, TweenMax);