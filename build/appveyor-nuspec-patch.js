updateDependency('UmbracoFileSystemProviders.Azure.Media.nuspec');
updateDependency('UmbracoFileSystemProviders.Azure.Forms.nuspec');

function updateDependency(fileName) {
    var read = require('read-file');
    var buffer = read.sync(fileName, { encoding: 'utf8' });

    var DomParser = require('@xmldom/xmldom').DOMParser;
    var XmlSerializer = require('@xmldom/xmldom').XMLSerializer;

    var doc = new DomParser().parseFromString(
        buffer
        , 'text/xml');

    var mssemver = process.env.mssemver;

    var dependenciesElement = doc.getElementsByTagName("metadata")[0].getElementsByTagName("dependencies")[0];
    var dependencies = dependenciesElement.getElementsByTagName("dependency");

    for (i = 0; i < dependencies.length; i++) {
        if (dependencies[i].getAttribute('id') === 'UmbracoFileSystemProviders.Azure') {
            dependencies[i].setAttribute('version', mssemver);
        }
    }

    var sXML = new XmlSerializer().serializeToString(doc);

    var writeFile = require('write');
    writeFile.sync(fileName, sXML);
}

