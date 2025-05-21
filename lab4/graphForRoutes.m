% File names
folder = '/Users/IceTea/Documents/Algoritmai/lab4/task3/bin/Debug/net7.0';
files = {
    fullfile(folder, 'data1.txt'),
    fullfile(folder, 'data2.txt'),
    fullfile(folder, 'data3.txt')
};
colors = {'r', 'g', 'b'}; % Red, Green, Blue

figure; hold on; grid on;
xlabel('X'); ylabel('Y');
title('Connected Dots from Files');

for i = 1:length(files)
    % Read data from file
    data = load(files{i}); % Assumes plain text with two columns
    x = data(:,1);
    y = data(:,2);
    
    % Plot lines and points
    plot(x, y, 'o-', 'Color', colors{i}, 'LineWidth', 1.5, 'MarkerSize', 6);

    plot(x(1), y(1), 'ko', 'MarkerFaceColor', 'k', 'MarkerSize', 8);
end

legend('File 1', 'File 2', 'File 3');
