# BE-FIN-028 — Resolver jenjang approval bersama (pembayaran, PO, Purchasing Invoice)

- TASK ID: BE-FIN-028
- TASK TYPE: TOUCHED LEGACY — ekstraksi logika existing ke helper bersama, plus koreksi satu nilai batas
- COMPLEXITY: LIGHT
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Payable/**`
- FILES INSPECTED:
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` (762 baris) — method `ResolveApprovalTier` (baris 649-659 sebelum perubahan), class `ApprovalTiers` (baris 750-754 sebelum perubahan), dua titik pemanggilan (baris 166, 333)
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` — komentar invariant `ApprovalTier`
  - `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` — komentar kelas soal status ambang
  - `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — submodul `Payable` sudah `ACTIVE`, prefix `Fin`; tidak ada entity operasional baru pada task ini (murni service, bukan model persisted), sehingga QBE-MOD-002/003 tidak berlaku
  - `docs/module-blueprints/finance-management/02-backend-architecture.md` — `FIN-DES-039` (dikoreksi 25 September 2026) dan blok "KOREKSI 25 September 2026" setelahnya
  - `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-052` (approved)
  - Pencarian menyeluruh `ApprovalTiers.`/`ResolveApprovalTier(` di seluruh repository — dikonfirmasi nol pemanggil lain di luar `FinancePaymentService.cs`
- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalTierResolver.cs` — `ApprovalTiers` (`Tier1`/`Tier2`, dipindah apa adanya) dan `FinanceApprovalTierResolver.Resolve(decimal totalAmount)` (method publik baru, menggantikan method privat lama)
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` — dua titik pemanggilan diarahkan ke resolver baru; method `ResolveApprovalTier` privat dan class `ApprovalTiers` lokal dihapus; komentar `FIN-OQ-010`/"provisional" diperbarui
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` — dua baris komentar XML diperbarui (bukan logika/kolom)
  - `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` — satu blok komentar kelas diperbarui (bukan endpoint/route/DTO)
- IMPLEMENTATION: Logika jenjang approval (`ResolveApprovalTier`) yang sebelumnya privat di dalam `FinancePaymentService` dipindah menjadi `FinanceApprovalTierResolver.Resolve(decimal)` publik di file terpisah pada submodul `Payable` yang sama, supaya dapat dipakai ulang oleh `FinancePurchaseOrderService` dan `FinancePurchasingInvoiceService` (`BE-FIN-032`, `034`) tanpa menyalin logikanya. Parameter `paymentType` pada signature lama **dihapus** — pembacaan source membuktikan parameter itu tidak pernah dipakai di dalam logika resolver (`switch` hanya menguji `totalAmount`), dan `FIN-DEC-052` menegaskan satu ambang seragam untuk seluruh jenis transaksi, sehingga mempertahankan parameter yang tidak terpakai pada resolver bersama tidak beralasan.

  **Satu nilai batas dikoreksi**, bukan sekadar dipindah: kode lama memakai `<= 50.000.000m => TIER_1` (provisional, komentarnya sendiri menyebut "menunggu ratifikasi FIN-OQ-010"). `FIN-DEC-052` yang sudah `approved` menetapkan `< Rp 50.000.000` disetujui Supervisor Finance dan `>= Rp 50.000.000` (termasuk tepat Rp 50.000.000) disetujui Manajer Finance. Resolver baru memakai `< 50_000_000m => TIER_1, _ => TIER_2` — hanya nilai **tepat** Rp 50.000.000 yang berpindah tier (dari `TIER_1` ke `TIER_2`); seluruh nilai lain tidak berubah hasilnya.

  Registrasi DI **tidak ditambahkan**: `FinanceApprovalTierResolver` dan `ApprovalTiers` adalah `static class` tanpa dependency maupun state (persis sifat method dan class yang digantikannya, yang juga `private static`/`public static`), sehingga tidak ada yang perlu didaftarkan ke DI container. Instruksi task menyebut "ikuti pola `AddScoped` existing" — pemeriksaan source membuktikan itu tidak berlaku di sini karena resolvernya stateless, bukan diinjeksi lewat constructor manapun.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Nol. Tidak ada endpoint, route, DTO, atau nilai enum yang berubah bentuk. `PaymentDetailResponse.approvalTier` tetap `string` `"TIER_1"`/`"TIER_2"` seperti sebelumnya — hanya *nilai* yang mungkin berubah untuk nominal tepat Rp 50.000.000, bukan bentuknya.
- DATABASE IMPACT: Nol. Tidak ada kolom, migration, atau perubahan skema. `FinPayment.ApprovalTier` tetap kolom `string(30)?` yang sudah ada; nilainya ditulis sekali saat `Draft`/`Update` (baris 166, 333) dan **tidak** dihitung ulang saat dibaca — pembayaran yang sudah `SUBMITTED`/`APPROVED`/`PAID` sebelum perubahan ini tetap menyimpan tier lama apa adanya, sesuai `FIN-DEC-052` ("jejak audit MUST mencatat jenjang yang berlaku dan nominal transaksi pada saat approval diberikan, bukan dihitung ulang belakangan").
- SECURITY IMPACT: **Ada, disengaja dan disetujui.** Pembayaran (dan kelak PO/Purchasing Invoice) bernilai **tepat Rp 50.000.000** yang dibuat/diajukan **setelah** perubahan ini berlaku menuntut approval Manajer Finance, sebelumnya cukup Supervisor Finance. Ini perubahan otorisasi by design (`FIN-DEC-052`, approved 25 September 2026), bukan efek samping. Tidak ada perubahan pada `[AccessPermission]`, resource, atau action — jenjang tetap ditentukan oleh nilai `ApprovalTier` yang sudah ada, hanya perhitungannya yang berubah pada satu titik nominal.
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna ("tampa build automatis") — pengguna akan menjalankan `dotnet build` sendiri. **Ini bukan klaim PASS**; task ini belum terverifikasi oleh compiler |
  | Review manual — dua titik pemanggilan lama (`ResolveApprovalTier(normalizedPaymentType, totalAmount)` baris 166 dan `ResolveApprovalTier(payment.PaymentType, totalAmount)` baris 333) | Keduanya diganti `FinanceApprovalTierResolver.Resolve(totalAmount)` | PASS (review) | Baca ulang kedua baris pasca-edit |
  | Pencarian teks `ApprovalTiers\.\|ResolveApprovalTier\(` di seluruh repository | Hanya dua kecocokan, keduanya di dalam `FinanceApprovalTierResolver.cs` sendiri (definisi, bukan pemanggil lama) | PASS (review) | Nol sisa definisi ganda, nol pemanggil yang belum dialihkan |
  | Pencarian teks `FIN-OQ-010\|provisional` pada tiga berkas yang diedit | Nol kecocokan tersisa | PASS (review) | `grep` dijalankan pasca-edit |
  | Baca ulang penutup class `FinancePaymentService` (baris 736) dan struktur brace di sekitar bagian yang dihapus | Struktur tetap valid — satu blok komentar menggantikan method+class yang dihapus, tidak ada brace yang tertinggal atau hilang | PASS (review) | Baca manual baris 628-660 dan 730-736 pasca-edit |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED.** Risiko yang tersisa akibat tidak menjalankan `dotnet build` dicatat eksplisit pada `KNOWN ISSUES` di bawah, bukan disembunyikan.

- WARNINGS: Task ini **belum dikonfirmasi compile** oleh `dotnet build`. Risiko realistis yang diketahui rendah (perubahan berupa pemindahan dua definisi statis ke file baru dalam namespace yang sama, tanpa perubahan signature yang dipanggil dari luar file selain dua titik yang sudah diperbarui) tetapi tetap belum nol.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — ditandai eksplisit sebagai validasi tertunda milik pengguna, sesuai instruksi. **Task ini MUST NOT dianggap selesai penuh sampai pengguna mengonfirmasi build sukses.**
  2. Perubahan `SecurityImpact` (nominal tepat Rp 50.000.000 berpindah tier) berlaku hanya untuk pembayaran yang **dibuat/diajukan setelah** perubahan ini aktif di lingkungan yang bersangkutan — belum ada mekanisme migrasi data untuk baris lama, dan memang tidak diminta (`FIN-DEC-052` eksplisit menyatakan jenjang lama tetap berlaku untuk approval yang sudah diberikan).
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE saat ini — memerlukan `dotnet build` sukses lebih dulu (di luar cakupan task ini atas instruksi eksplisit pengguna). Skenario manual yang MUST dijalankan pengguna sebelum menandai task ini benar-benar selesai: (a) pembayaran Rp 49.999.999 → `TIER_1`; (b) pembayaran tepat Rp 50.000.000 → `TIER_2`; (c) pembayaran Rp 50.000.001 → `TIER_2`.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs
  M  Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs
  M  Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs
  ?? Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalTierResolver.cs
  ```

  (Perubahan lain pada `git status --short` keseluruhan repository — berkas `docs/module-blueprints/finance-management/**` dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — sudah ada sebelum task ini dimulai, dari `BE-FIN-027` dan pass desain sebelumnya; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build` untuk memverifikasi task ini** sebelum dianggap selesai penuh. Sesudah itu, `BE-FIN-029` (model Purchasing) dan `BE-FIN-038`/`041` (tanpa prasyarat) tersedia untuk dikerjakan; `BE-FIN-032` dan `BE-FIN-034` kelak memanggil `FinanceApprovalTierResolver.Resolve` yang sama persis dengan yang dipakai task ini.
