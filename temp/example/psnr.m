function [PSNR] = imreed(OrgImg,ModImg)
%source of code
%http://stackoverflow.com/questions/16264141/power-signal-noise-ratio-psnr-of-colored-jpeg-image/16265510#16265510
I = imread(OrgImg);
Ihat = imread(ModImg);

% Read the dimensions of the image.
[rows, columns ~] = size(I);

% Calculate mean square error of R, G, B.   
mseRImage = (double(I(:,:,1)) - double(Ihat(:,:,1))) .^ 2;
mseGImage = (double(I(:,:,2)) - double(Ihat(:,:,2))) .^ 2;
mseBImage = (double(I(:,:,3)) - double(Ihat(:,:,3))) .^ 2;

mseR = sum(sum(mseRImage)) / (rows * columns);
mseG = sum(sum(mseGImage)) / (rows * columns);
mseB = sum(sum(mseBImage)) / (rows * columns);

% Average mean square error of R, G, B.
MSE = (mseR + mseG + mseB)/3;

% Calculate PSNR (Peak Signal to noise ratio).
PSNR = 10 * log10( 255^2 / MSE);

end