#include <catch2/catch.hpp>
#include "../../src/core/collaboration/real_time_coauthoring.h"
#include "../../src/core/models/document.h"
#include "../../src/core/models/user.h"
#include <string>
#include <vector>
#include <memory>
#include <thread>
#include <chrono>

using namespace std::chrono_literals;

// Helper function to create a sample document for testing
std::shared_ptr<Document> createSampleDocument() {
    auto doc = std::make_shared<Document>();
    doc->setContent("This is a sample document for testing.");
    return doc;
}

// Helper function to create a sample user for testing
std::shared_ptr<User> createSampleUser(const std::string& userId) {
    return std::make_shared<User>(userId);
}

// Helper function to simulate network latency
void simulateNetworkLatency(std::chrono::milliseconds delay) {
    std::this_thread::sleep_for(delay);
}

// Helper function to verify document consistency across users
bool verifyDocumentConsistency(const std::vector<std::shared_ptr<User>>& users, const Document& expectedDocument) {
    for (const auto& user : users) {
        if (user->getDocument()->getContent() != expectedDocument.getContent()) {
            return false;
        }
    }
    return true;
}

TEST_CASE("RealTimeCoauthoring", "[collaboration]") {
    SECTION("UserJoinSession") {
        // Create a RealTimeCoauthoring object
        RealTimeCoauthoring coauthoring;
        
        // Create a sample Document
        auto document = createSampleDocument();
        
        // Create a User object
        auto user = createSampleUser("user1");
        
        // Join the user to the co-authoring session
        REQUIRE(coauthoring.joinSession(user, document));
        
        // Verify that the user is added to the active users list
        REQUIRE(coauthoring.getActiveUsers().size() == 1);
        REQUIRE(coauthoring.getActiveUsers()[0]->getId() == "user1");
    }

    SECTION("UserLeaveSession") {
        // Create a RealTimeCoauthoring object with an active user
        RealTimeCoauthoring coauthoring;
        auto user = createSampleUser("user1");
        auto document = createSampleDocument();
        coauthoring.joinSession(user, document);
        
        // Remove the user from the co-authoring session
        REQUIRE(coauthoring.leaveSession(user));
        
        // Verify that the user is removed from the active users list
        REQUIRE(coauthoring.getActiveUsers().empty());
    }

    SECTION("ApplyTextEdit") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Apply a text edit operation from one user
        TextEdit edit{0, 4, "That"};
        REQUIRE(coauthoring.applyEdit(user1, edit));
        
        // Verify that the edit is applied to the document
        REQUIRE(document->getContent().substr(0, 4) == "That");
        
        // Verify that other users receive the edit notification
        REQUIRE(user2->getDocument()->getContent() == document->getContent());
    }

    SECTION("ConcurrentEdits") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Simulate concurrent edit operations from different users
        TextEdit edit1{0, 4, "That"};
        TextEdit edit2{document->getContent().length(), 0, " Edited by user2."};
        
        std::thread t1([&]() { coauthoring.applyEdit(user1, edit1); });
        std::thread t2([&]() { coauthoring.applyEdit(user2, edit2); });
        
        t1.join();
        t2.join();
        
        // Verify that all edits are applied correctly
        std::string expectedContent = "That is a sample document for testing. Edited by user2.";
        REQUIRE(document->getContent() == expectedContent);
        
        // Verify that the final document state is consistent for all users
        REQUIRE(verifyDocumentConsistency({user1, user2}, *document));
    }

    SECTION("ConflictResolution") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Simulate overlapping edit operations from different users
        TextEdit edit1{0, 4, "That"};
        TextEdit edit2{0, 4, "Those"};
        
        std::thread t1([&]() { coauthoring.applyEdit(user1, edit1); });
        std::thread t2([&]() { coauthoring.applyEdit(user2, edit2); });
        
        t1.join();
        t2.join();
        
        // Verify that conflicts are detected and resolved
        // Note: The exact resolution strategy may vary, here we assume last-write-wins
        REQUIRE(document->getContent().substr(0, 5) == "Those");
        
        // Verify that the resolution is communicated to all users
        REQUIRE(verifyDocumentConsistency({user1, user2}, *document));
    }

    SECTION("CursorPositionBroadcast") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Update cursor position for one user
        size_t cursorPosition = 10;
        REQUIRE(coauthoring.updateCursorPosition(user1, cursorPosition));
        
        // Verify that other users receive the cursor position update
        REQUIRE(coauthoring.getUserCursorPosition(user2, user1->getId()) == cursorPosition);
    }

    SECTION("SelectionBroadcast") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Make a text selection for one user
        TextSelection selection{5, 10};
        REQUIRE(coauthoring.updateSelection(user1, selection));
        
        // Verify that other users receive the selection information
        auto receivedSelection = coauthoring.getUserSelection(user2, user1->getId());
        REQUIRE(receivedSelection.start == selection.start);
        REQUIRE(receivedSelection.end == selection.end);
    }

    SECTION("LargeEditPerformance") {
        // Create a RealTimeCoauthoring object with a large document
        RealTimeCoauthoring coauthoring;
        auto largeDocument = std::make_shared<Document>();
        largeDocument->setContent(std::string(1000000, 'a')); // 1MB of 'a's
        auto user = createSampleUser("user1");
        coauthoring.joinSession(user, largeDocument);
        
        // Apply a large edit operation
        TextEdit largeEdit{500000, 0, std::string(100000, 'b')}; // Insert 100KB of 'b's in the middle
        
        // Measure the time taken to apply and broadcast the edit
        auto start = std::chrono::high_resolution_clock::now();
        REQUIRE(coauthoring.applyEdit(user, largeEdit));
        auto end = std::chrono::high_resolution_clock::now();
        
        // Verify that the operation completes within an acceptable time frame (e.g., 1 second)
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end - start);
        REQUIRE(duration.count() < 1000);
    }

    SECTION("NetworkLatencySimulation") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Introduce artificial delay in message passing
        coauthoring.setNetworkLatencySimulator([](){ simulateNetworkLatency(100ms); });
        
        // Perform various edit operations
        TextEdit edit1{0, 4, "That"};
        TextEdit edit2{document->getContent().length(), 0, " Edited with latency."};
        
        std::thread t1([&]() { coauthoring.applyEdit(user1, edit1); });
        std::thread t2([&]() { coauthoring.applyEdit(user2, edit2); });
        
        t1.join();
        t2.join();
        
        // Verify that the system handles latency gracefully and maintains consistency
        std::string expectedContent = "That is a sample document for testing. Edited with latency.";
        REQUIRE(document->getContent() == expectedContent);
        REQUIRE(verifyDocumentConsistency({user1, user2}, *document));
    }

    SECTION("RecoverFromDisconnection") {
        // Create a RealTimeCoauthoring object with multiple users
        RealTimeCoauthoring coauthoring;
        auto document = createSampleDocument();
        auto user1 = createSampleUser("user1");
        auto user2 = createSampleUser("user2");
        coauthoring.joinSession(user1, document);
        coauthoring.joinSession(user2, document);
        
        // Simulate a temporary disconnection for one user
        coauthoring.simulateDisconnection(user2);
        
        // Perform edits during the disconnection
        TextEdit edit{0, 4, "That"};
        REQUIRE(coauthoring.applyEdit(user1, edit));
        
        // Reconnect the user
        coauthoring.simulateReconnection(user2);
        
        // Verify that the reconnected user's document is synchronized correctly
        REQUIRE(user2->getDocument()->getContent() == document->getContent());
    }
}