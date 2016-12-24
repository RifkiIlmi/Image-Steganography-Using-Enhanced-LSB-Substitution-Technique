function [capacity] = calcByteSize (impath)
capacity = 0;
image = imread(impath);
clc;
Red = imhist(image(:,:,1));
Blue = imhist(image(:,:,2));
Green = imhist(image(:,:,3));
for i = 1:256
        p = (ceil(log2(i)));
        if(p>=7)
            capacity = capacity + (Red(i)+Green(i)+Blue(i))*4;
            
        elseif(p<=6 && p > 4 )
            capacity = capacity + (Red(i)+Green(i)+Blue(i))*3;
            
        
        elseif(p<=4 && p > 2 )
            capacity = capacity + (Red(i)+Green(i)+Blue(i))*2;
            
        elseif(p<=2 && p>=0 )
            capacity = capacity + (Red(i)+Green(i)+Blue(i));
            
        end
end

end