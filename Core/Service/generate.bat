@echo off
cd /d "%~dp0"

set svcutil="SvcUtil.exe"

set onvifXSD=http://www.onvif.org/ver10/schema/onvif.xsd
set commonXSD=http://www.onvif.org/ver10/schema/common.xsd
set deviceWSDL=http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl
set ptzWDL=http://www.onvif.org/onvif/ver20/ptz/wsdl/ptz.wsdl
set mediaWDL=http://www.onvif.org/ver10/media/wsdl/media.wsdl
set imagingWDL=http://www.onvif.org/ver20/imaging/wsdl/imaging.wsdl

set outputFile=OnvifService.cs
set namespace=Onvif.Core

del %outputFile%
%svcutil% %onvifXSD% %commonXSD% %deviceWSDL% %ptzWDL% %mediaWDL% %imagingWDL% /out:%outputFile% /namespace:*,%namespace% /syncOnly /noConfig

pause