using Xunit;

// Menonaktifkan paralelisme antarkelas test pada assembly ini.
//
// Alasannya bukan kerapian, melainkan kebenaran hasil. Seluruh test berbasis database di sini
// menulis ke satu database yang sama, dan BillingFolioService sengaja memakai isolation level
// Serializable karena itulah yang menjamin satu fakta klinis tidak pernah menjadi dua charge.
// Ketika tiga kelas test berjalan bersamaan, transaksi-transaksi itu saling menyerobot,
// PostgreSQL menggagalkan salah satunya dengan serialization failure, dan setelah percobaan
// ulang habis service mengembalikan BIL_OUTCOME_UNKNOWN.
//
// Hasilnya adalah kegagalan yang menyesatkan: test tampak menemukan cacat domain, padahal yang
// terjadi adalah test saling mengganggu. Bukti bahwa penyebabnya memang paralelisme —
// ClinicalMilestoneFactProducerTests gagal 3 kali dalam satu proses bersama, lalu lulus 11 dari
// 11 ketika dijalankan sendirian.
//
// Menaikkan jumlah percobaan ulang pada service adalah jawaban yang salah: itu akan menutupi
// gejala pada kode produksi demi kenyamanan test. Yang benar adalah tidak menjalankan test
// yang berbagi satu database secara bersamaan.
//
// Biayanya kecil. Seluruh test murni pada assembly ini selesai dalam puluhan milidetik,
// sehingga menjalankannya berurutan tidak terasa.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
