#include <stdio.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdlib.h>
#include <unistd.h>

#define PORT 8080

void beginAccepting(int serverfd) {
    int clientfd = accept(serverfd, NULL, 0);
    char buffer[1024];
    ssize_t bytesRead = read(clientfd, buffer, 1023);
    buffer[bytesRead] = 0;
    printf("message: %s\n", buffer);






    close(clientfd);
    puts("closed client connection");
}

int main() {
    int socketfd = socket(AF_INET, SOCK_STREAM, 0);
    struct sockaddr_in address;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_family = AF_INET;
    address.sin_port = htons(PORT);

    if (socketfd < 0) {
        perror("socket creation failed");
        exit(EXIT_FAILURE);
    }
    puts("socket created");

    if (bind(socketfd, (struct sockaddr*)&address, sizeof(address)) < 0) {
        perror("error binding socket");
        exit(EXIT_FAILURE);
    }
    puts("socket binded successfully");

    if(listen(socketfd, 5) < 0) {
        perror("error setting socket to listen");
        exit(EXIT_FAILURE);
    }
    puts("socket is listening");

    beginAccepting(socketfd);

    close(socketfd);
}
