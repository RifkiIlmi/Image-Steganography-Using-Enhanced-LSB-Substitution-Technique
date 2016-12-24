function [capacity] = calcByteSize (impath)
capacity = 0;
image = imread(impath);
[M,N,A]=size(image);
capacity = M*N*A;
end