namespace EquipmentBooking.DbSeeder;

/// <summary>Fixed demo catalogue: departments, locations, features, categories and item models.</summary>
internal static class DemoCatalogue
{
    public static readonly string[] Departments =
    [
        "Engineering", "Marketing", "Sales", "Finance", "Human Resources", "IT Support", "Research", "Operations",
    ];

    public static readonly string[] Locations =
    [
        "Main storage room", "Floor 2 equipment cabinet", "Floor 3 media room", "Warehouse", "Laboratory 1", "Reception desk",
    ];

    public static readonly string[] Features =
    [
        "4K", "Bluetooth", "GPS", "HDMI", "USB-C", "Wireless", "Waterproof", "Rechargeable", "Portable", "Noise cancelling",
        "Tripod mount", "Zoom lens", "Touchscreen", "Long battery life", "Outdoor use", "High resolution", "Data logging",
        "Lightweight", "Weather sealed", "Multi-channel",
    ];

    public static readonly CategorySpec[] Categories =
    [
        new("Laptops", "Portable computers for business trips and temporary replacements.", 14,
            "Business trips, temporary replacement of a broken work laptop, presentations and training outside the office.", false,
            [
                new("Dell Latitude 5440", 1150m, ["USB-C", "HDMI", "Long battery life", "Lightweight"]),
                new("Lenovo ThinkPad T14 Gen 4", 1290m, ["USB-C", "HDMI", "Long battery life"]),
                new("HP EliteBook 840 G10", 1240m, ["USB-C", "HDMI", "Lightweight"]),
                new("Apple MacBook Air 13 M2", 1199m, ["USB-C", "Lightweight", "Long battery life"]),
            ], 14),
        new("Projectors", "Projectors for meeting rooms and events.", 5,
            "Presentations in meeting rooms, client meetings, training sessions and internal events.", false,
            [
                new("Epson EB-W51", 449m, ["HDMI", "Portable"]),
                new("BenQ MW560", 399m, ["HDMI", "Portable"]),
                new("Optoma HD29i", 629m, ["HDMI", "High resolution"]),
            ], 12),
        new("Cameras", "Photo and video cameras.", 7,
            "Recording interviews, events, product photos and site documentation for work purposes.", false,
            [
                new("Canon EOS R50", 799m, ["4K", "Zoom lens", "Wireless"]),
                new("Sony ZV-E10", 699m, ["4K", "Wireless", "Lightweight"]),
                new("GoPro HERO12 Black", 449m, ["4K", "Waterproof", "Outdoor use", "Rechargeable"]),
                new("Nikon Z50", 899m, ["4K", "Zoom lens", "Weather sealed"]),
            ], 14),
        new("Audio recorders", "Portable audio recorders.", 7,
            "Recording interviews, meetings with consent of the participants, and field audio for work projects.", false,
            [
                new("Zoom H5", 279m, ["Portable", "Multi-channel"]),
                new("Tascam DR-40X", 189m, ["Portable", "Rechargeable"]),
                new("Sony ICD-UX570", 99m, ["Portable", "Lightweight", "USB-C"]),
            ], 10),
        new("Microphones", "Wireless and USB microphones.", 7,
            "Presentations, webinars, recording of training videos and events.", false,
            [
                new("Rode Wireless GO II", 299m, ["Wireless", "Rechargeable", "Lightweight"]),
                new("Shure MV7", 249m, ["USB-C", "Noise cancelling"]),
                new("Blue Yeti", 129m, ["USB-C", "Multi-channel"]),
            ], 10),
        new("Conference kits", "Speakerphones and video bars for remote meetings.", 5,
            "Remote meetings with clients and colleagues in rooms without built-in conference equipment.", false,
            [
                new("Jabra Speak2 75", 349m, ["Bluetooth", "Wireless", "Portable"]),
                new("Logitech MeetUp", 899m, ["4K", "USB-C"]),
                new("Poly Studio P15", 599m, ["4K", "USB-C", "Noise cancelling"]),
            ], 8),
        new("Tablets", "Tablets for demonstrations and field work.", 10,
            "Product demonstrations, digital forms during site visits and testing of applications.", false,
            [
                new("Apple iPad 10th generation", 529m, ["Touchscreen", "Wireless", "Long battery life"]),
                new("Samsung Galaxy Tab S9", 799m, ["Touchscreen", "Waterproof", "High resolution"]),
            ], 10),
        new("Monitors", "Additional displays for temporary workplaces.", 30,
            "Temporary second screen at a workplace, onboarding of new employees and hot desks.", false,
            [
                new("Dell P2423D", 289m, ["HDMI", "High resolution"]),
                new("LG 27UL500", 319m, ["4K", "HDMI"]),
            ], 10),
        new("Headsets", "Headsets for calls and training.", 14,
            "Calls with clients, online training and work in open spaces.", false,
            [
                new("Jabra Evolve2 65", 219m, ["Bluetooth", "Wireless", "Noise cancelling"]),
                new("Poly Voyager Focus 2", 269m, ["Bluetooth", "Noise cancelling", "Rechargeable"]),
            ], 12),
        new("Measuring instruments", "Calibrated measuring instruments.", 7,
            "Measurements on customer sites and in the laboratory by trained employees.", true,
            [
                new("Fluke 117 multimeter", 289m, ["Portable", "Data logging"]),
                new("Bosch GLM 50-27 laser distance meter", 159m, ["Bluetooth", "Portable"]),
                new("Testo 865 thermal camera", 1490m, ["High resolution", "Data logging"]),
            ], 12),
        new("Test devices", "Smartphones and devices for software testing.", 14,
            "Testing of mobile applications and websites on real devices.", false,
            [
                new("Google Pixel 8", 699m, ["Touchscreen", "USB-C", "GPS"]),
                new("Apple iPhone 14", 799m, ["Touchscreen", "GPS", "Waterproof"]),
                new("Samsung Galaxy A54", 449m, ["Touchscreen", "USB-C", "GPS"]),
            ], 12),
        new("Drones", "Camera drones for site inspections.", 3,
            "Site inspections and aerial photos by certified pilots in permitted areas only.", true,
            [
                new("DJI Mini 4 Pro", 959m, ["4K", "GPS", "Outdoor use"]),
                new("DJI Air 3", 1099m, ["4K", "GPS", "Outdoor use", "Long battery life"]),
            ], 4),
        new("Lighting", "Portable lights for photo and video.", 7,
            "Lighting for video recordings, photo shoots and events.", false,
            [
                new("Godox SL60W", 139m, ["Tripod mount"]),
                new("Aputure Amaran 100d", 199m, ["Tripod mount", "Wireless"]),
            ], 8),
        new("Network test tools", "Tools for testing cabling and networks.", 5,
            "Network diagnostics and cable certification by IT staff.", true,
            [
                new("Fluke LinkIQ cable tester", 1890m, ["Portable", "Data logging"]),
                new("NetAlly LinkRunner AT 4000", 2390m, ["Portable", "Data logging"]),
            ], 4),
    ];
}

internal sealed record CategorySpec(
    string Name,
    string Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    ModelSpec[] Models,
    int ItemCount);

internal sealed record ModelSpec(string Name, decimal Value, string[] Features);
